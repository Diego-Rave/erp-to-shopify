using ErpToShopify.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using ErpToShopify.Models;

namespace ErpToShopify.Functions;

public class SyncIntegratorFunction
{
    private readonly IBnetService _bnetService;
    private readonly IShopifyService _shopifyService;
    private readonly ILogger _logger;

    // Inyección de dependencias adaptada al modelo Aislado
    public SyncIntegratorFunction(ILoggerFactory loggerFactory, IBnetService bnetService, IShopifyService shopifyService)
    {
        _logger = loggerFactory.CreateLogger<SyncIntegratorFunction>();
        _bnetService = bnetService;
        _shopifyService = shopifyService;
    }

    // En el modelo aislado usamos [Function] en lugar de [FunctionName]
    [Function("SyncBnetToShopify_MVP")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Iniciando sincronización BNET -> Shopify (MVP).");

        // 1. EXTRAER (Mock)
        var productosBnet = await _bnetService.ObtenerProductosActivosAsync();

        // 2. TRANSFORMAR
        var payloadShopify = productosBnet.Select(p => new ShopifyUpdateRequest
        {
            Sku = p.Codigo,
            Titulo = p.Descripcion,
            Price = p.Precio,
            InventoryQuantity = p.SaldoBodega,
            ShopifyVariantId = p.ShopifyVariantId

        }).ToList();

        // 3. LA TOMA DE DECISIONES (Camino A y B)
        var productosParaActualizar = new List<ShopifyUpdateRequest>();

        foreach (var prod in payloadShopify)
        {
            if (string.IsNullOrEmpty(prod.ShopifyVariantId))
            {
                // CAMINO B: No existe el ID. Lo creamos en Shopify.
                // Recibimos la Tupla (Los dos IDs al mismo tiempo)
                var (nuevoProductId, nuevoVariantId) = await _shopifyService.CrearProductoAsync(prod);

                if (!string.IsNullOrEmpty(nuevoProductId) && !string.IsNullOrEmpty(nuevoVariantId))
                {
                    // Guardamos AMBOS IDs en SQL
                    await _bnetService.GuardarNuevoIdHomologadoAsync(prod.Sku, nuevoProductId, nuevoVariantId);

                    prod.ShopifyVariantId = nuevoVariantId;
                    productosParaActualizar.Add(prod);
                }
            }
            else
            {
                // CAMINO A: Ya existe, va directo a la lista de actualización
                productosParaActualizar.Add(prod);
            }
        }

        // 4. LOAD: Enviamos las actualizaciones por Lotes (Chunking)
        if (productosParaActualizar.Any())
        {
            await _shopifyService.ActualizarInventarioYPrecioAsync(productosParaActualizar);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { Mensaje = "Sincronización exitosa", ProductosProcesados = productosParaActualizar.Count });
        return response;
    }
}
