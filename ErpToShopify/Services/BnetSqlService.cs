using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ErpToShopify.Interfaces;
using ErpToShopify.Models;

namespace ErpToShopify.Services
{
    public class BnetSqlService : IBnetService
    {
        private readonly string _connectionString;

        public BnetSqlService(IConfiguration configuration)
        {
            _connectionString = configuration["BnetConnectionString"];
        }

        public async Task<List<BnetProduct>> ObtenerProductosActivosAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            // Hacemos un INNER JOIN para traer los datos cruzados
            string sql = @"
                    SELECT 
                        p.CODIGO as Codigo, 
                        p.DESCRIPCION as Descripcion, 
                        p.PRECIO as Precio, 
                        p.SALDO as SaldoBodega,
                        b.Nombre as NombreBodega,
                        h.ShopifyVariantId as ShopifyVariantId -- ¡El ID nativo de Shopify!
                    FROM Productos p
                    INNER JOIN Bodegas b ON p.BodegaId = b.Id
                    LEFT JOIN HomologacionShopify h ON p.CODIGO = h.SKU
                    WHERE p.ACTIVO = 1";

            // Dapper ejecuta la consulta y construye la lista de objetos por nosotros
            var productos = await connection.QueryAsync<BnetProduct>(sql);

            // Solo para verificar que funcionó, imprimimos en consola lo que trajimos de SQL
            Console.WriteLine("--- DATOS EXTRAÍDOS DE SQL SERVER LOCAL ---");
            foreach (var prod in productos)
            {
                Console.WriteLine($"Extraído: {prod.Codigo} - {prod.Descripcion} | {prod.SaldoBodega} unds en {prod.NombreBodega}");
            }

            return productos.ToList();
        }

        public async Task<bool> CrearPedidoAsync(ShopifyOrderPayload pedido)
        {
            // Aquí simularías el INSERT en las tablas de pedidos de BNET
            return true;
        }

        public async Task GuardarHomologacionAsync(List<HomologacionDto> diccionario)
        {
            using var connection = new SqlConnection(_connectionString);

            // Instrucción MERGE: Si el SKU existe, lo actualiza. Si no existe, lo inserta.
            string sql = @"
        MERGE INTO HomologacionShopify AS Destino
        USING (SELECT @Sku AS SKU, @ShopifyVariantId AS ShopifyVariantId) AS Origen
        ON Destino.SKU = Origen.SKU
        WHEN MATCHED THEN
            UPDATE SET ShopifyVariantId = Origen.ShopifyVariantId
        WHEN NOT MATCHED THEN
            INSERT (SKU, ShopifyVariantId)
            VALUES (Origen.SKU, Origen.ShopifyVariantId);";

            // ¡La magia de Dapper! Le pasamos la lista completa y él hace el bucle de inserción
            await connection.ExecuteAsync(sql, diccionario);

            Console.WriteLine("Diccionario guardado exitosamente en SQL Server.");
        }

        public async Task GuardarNuevoIdHomologadoAsync(string sku, string productId, string variantId)
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);

            string sql = @"
        INSERT INTO HomologacionShopify (SKU, ShopifyProductId, ShopifyVariantId) 
        VALUES (@Sku, @ProductId, @VariantId)";

            await connection.ExecuteAsync(sql, new { Sku = sku, ProductId = productId, VariantId = variantId });
            Console.WriteLine($"[SQL] IDs de Padre e Hijo guardados para {sku}");
        }
    }
}