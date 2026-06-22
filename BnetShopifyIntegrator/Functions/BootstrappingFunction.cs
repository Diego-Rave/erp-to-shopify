using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using BnetShopifyIntegrator.Interfaces;

namespace BnetShopifyIntegrator.Functions
{
    public class BootstrappingFunction
    {
        private readonly IShopifyService _shopifyService;
        private readonly IBnetService _bnetService;

        public BootstrappingFunction(IShopifyService shopifyService, IBnetService bnetService)
        {
            _shopifyService = shopifyService;
            _bnetService = bnetService;
        }

        [Function("Sync_Bootstrapping")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "sync/bootstrap")] HttpRequestData req)
        {
            // 1. Extraer el diccionario completo de Shopify
            var diccionario = await _shopifyService.ExtraerDiccionarioIdsAsync();

            if (diccionario.Any())
            {
                // 2. Guardarlo en la tabla SQL de BNET
                await _bnetService.GuardarHomologacionAsync(diccionario);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Bootstrapping finalizado. {diccionario.Count} IDs sincronizados en SQL.");
            return response;
        }
    }
}