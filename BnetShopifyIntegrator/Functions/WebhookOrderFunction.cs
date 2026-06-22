using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using BnetShopifyIntegrator.Interfaces;
using BnetShopifyIntegrator.Models;

namespace BnetShopifyIntegrator.Functions
{
    public class WebhookOrderFunction
    {
        private readonly IBnetService _bnetService;
        private readonly ILogger _logger;

        public WebhookOrderFunction(ILoggerFactory loggerFactory, IBnetService bnetService)
        {
            _logger = loggerFactory.CreateLogger<WebhookOrderFunction>();
            _bnetService = bnetService; // Solo inyectamos BNET, no necesitamos ShopifyService aquí
        }

        [Function("Webhook_ShopifyOrder")]
        // Nota que solo aceptamos POST y definimos una ruta limpia
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhooks/orders/create")] HttpRequestData req)
        {
            _logger.LogInformation("Webhook disparado: Recibiendo nuevo pedido de Shopify.");

            try
            {
                // 1. Leer el JSON que nos envía Shopify en el cuerpo de la petición
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                // 2. Transformar el JSON a nuestro Modelo C#
                var pedido = JsonSerializer.Deserialize<ShopifyOrderPayload>(requestBody);

                if (pedido == null)
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                // 3. Cargar (Load) el pedido en BNET
                var exito = await _bnetService.CrearPedidoAsync(pedido);

                // 4. Responder a Shopify con un 200 OK
                // Si no le respondemos 200 OK rápido, Shopify pensará que fallamos y reintentará enviarlo.
                var response = req.CreateResponse(exito ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error procesando webhook: {ex.Message}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
