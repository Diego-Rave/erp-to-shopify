using System.Collections.Generic;
using System.Threading.Tasks;
using ErpToShopify.Interfaces; // Para que reconozca IBnetService
using ErpToShopify.Models;

namespace ErpToShopify.Services
{
    public class BnetMockService 
    {
        public async Task<List<BnetProduct>> ObtenerProductosActivosAsync()
        {
            // Simulamos el tiempo de respuesta de una base de datos real
            await Task.Delay(500);

            // Aquí simulamos lo que a futuro será tu consulta SQL a BNET
            // Agregué un tercer producto con stock 0 para hacer pruebas más reales
            return new List<BnetProduct>
            {
                new BnetProduct { Codigo = "SKU-001", Descripcion = "Camiseta Básica", Precio = 45000, SaldoBodega = 150 },
                new BnetProduct { Codigo = "SKU-002", Descripcion = "Pantalón Denim", Precio = 120000, SaldoBodega = 45 },
                new BnetProduct { Codigo = "SKU-003", Descripcion = "Chaqueta Cuero", Precio = 250000, SaldoBodega = 0 }
            };
        }

        // Implementación de Crear Pedido
        public async Task<bool> CrearPedidoAsync(ShopifyOrderPayload pedidoShopify)
        {
            // Simulación el tiempo de inserción en SQL
            await Task.Delay(300);

            Console.WriteLine($"\n--- NUEVO PEDIDO RECIBIDO PARA BNET ---");
            Console.WriteLine($"Orden #: {pedidoShopify.OrderNumber}");
            Console.WriteLine($"Total: ${pedidoShopify.TotalPrice}");
            Console.WriteLine("Artículos:");

            foreach (var item in pedidoShopify.LineItems)
            {
                Console.WriteLine($"- SKU: {item.Sku} | Cantidad: {item.Quantity} | Precio Unitario: ${item.Price}");
            }
            Console.WriteLine($"---------------------------------------\n");

            return true;
        }
    }
}
