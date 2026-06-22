using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BnetShopifyIntegrator.Interfaces;
using BnetShopifyIntegrator.Models;

namespace BnetShopifyIntegrator.Services
{
    public class BnetApiService : IBnetService
    {
        private readonly HttpClient _httpClient;

        public BnetApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 1. Obtener catálogo (Cumple: Task<List<BnetProduct>>)
        public async Task<List<BnetProduct>> ObtenerProductosActivosAsync()
        {
            Console.WriteLine("[BNET API] Solicitando catálogo de productos a FastAPI...");

            var response = await _httpClient.GetAsync("/api/productos/activos");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            try
            {
                // Estrategia A: Si FastAPI devuelve el JSON envuelto en { "data": [...] }
                using var document = JsonDocument.Parse(jsonResponse);
                if (document.RootElement.TryGetProperty("data", out var dataElement))
                {
                    return JsonSerializer.Deserialize<List<BnetProduct>>(dataElement.GetRawText()) ?? new List<BnetProduct>();
                }

                // Estrategia B: Si FastAPI devuelve directamente el arreglo [...]
                return JsonSerializer.Deserialize<List<BnetProduct>>(jsonResponse) ?? new List<BnetProduct>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR JSON] No se pudo deserializar la respuesta de BNET: {ex.Message}");
                return new List<BnetProduct>();
            }
        }

        // 2. Enviar Pedido (Cumple: Task<bool> CrearPedidoAsync)
        public async Task<bool> CrearPedidoAsync(ShopifyOrderPayload pedidoShopify)
        {
            Console.WriteLine($"[BNET API] Enviando pedido a FastAPI...");

            var response = await _httpClient.PostAsJsonAsync("/api/pedidos/nuevo", pedidoShopify);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[BNET API] Pedido insertado exitosamente en ERP.");
                return true;
            }

            Console.WriteLine($"[ERROR BNET] Falló la creación del pedido. Status: {response.StatusCode}");
            return false;
        }

        // 3. Guardar Homologación Masiva (Cumple: Task GuardarHomologacionAsync)
        public async Task GuardarHomologacionAsync(List<HomologacionDto> diccionario)
        {
            Console.WriteLine($"[BNET API] Enviando lote de {diccionario.Count} homologaciones a FastAPI...");

            var response = await _httpClient.PutAsJsonAsync("/api/productos/homologacion/batch", diccionario);
            response.EnsureSuccessStatusCode();
        }

        // 4. Guardar Tupla Padre/Hijo (Cumple: Task GuardarNuevoIdHomologadoAsync)
        public async Task GuardarNuevoIdHomologadoAsync(string sku, string productId, string variantId)
        {
            Console.WriteLine($"[BNET API] Enviando nuevos IDs de {sku} al ERP...");

            var payload = new
            {
                shopify_product_id = productId,
                shopify_variant_id = variantId
            };

            var response = await _httpClient.PutAsJsonAsync($"/api/productos/homologacion/{sku}", payload);
            response.EnsureSuccessStatusCode();

            Console.WriteLine($"[BNET API] ¡IDs de {sku} guardados exitosamente!");
        }
    }
}