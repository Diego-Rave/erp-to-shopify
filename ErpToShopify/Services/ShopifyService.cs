using ErpToShopify.Interfaces;
using ErpToShopify.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;

namespace ErpToShopify.Services
{
    public class ShopifyService : IShopifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _shopifyUrl;
        private readonly string _shopifyToken;

        public ShopifyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _shopifyUrl = configuration["ShopifyUrl"];
            _shopifyToken = configuration["ShopifyToken"];

            _httpClient.BaseAddress = new Uri($"https://{_shopifyUrl}/admin/api/2024-01/graphql.json");
            _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _shopifyToken);
        }

        public async Task<bool> ActualizarInventarioYPrecioAsync(List<ShopifyUpdateRequest> productos)
        {
            Console.WriteLine($"Iniciando carga OPTIMIZADA a Shopify de {productos.Count} productos...");

            // 1. CHUNKING: Dividimos la lista gigante en lotes manejables (ej. 50 productos por lote)
            int tamañoLote = 50;
            var lotes = productos.Chunk(tamañoLote);

            int loteActual = 1;

            foreach (var lote in lotes)
            {
                Console.WriteLine($"Procesando Lote {loteActual} de {lotes.Count()}...");

                // 2. CONCURRENCIA: Preparamos las tareas del lote para que corran al mismo tiempo
                // Usamos LINQ para convertir cada producto en una Tarea (Task) asíncrona
                var tareasDePeticion = lote.Select(producto => EnviarUnSoloProductoAsync(producto));

                // 3. EJECUCIÓN SIMULTÁNEA: Disparamos las 50 peticiones a la vez y esperamos a que todas terminen
                await Task.WhenAll(tareasDePeticion);

                // 4. THROTTLING (Acelerador): Pausamos 1 segundo antes de enviar el siguiente lote
                // Esto le da respiro a la API de Shopify y evita que nos bloqueen (Error 429)
                await Task.Delay(1000);

                loteActual++;
            }

            return true;
        }

        private async Task EnviarUnSoloProductoAsync(ShopifyUpdateRequest producto)
        {
            if (string.IsNullOrEmpty(producto.ShopifyVariantId)) return;

            try
            {
                Console.WriteLine($"[SHOPIFY] Obteniendo permisos y Producto Padre para: {producto.Sku}...");

                // PASO 1: Preguntarle a Shopify el ID del Producto (Requisito del nuevo API)
                var queryGet = @"
            query obtenerProductoPadre($id: ID!) {
              productVariant(id: $id) {
                product { id }
              }
            }";

                var payloadGet = new { query = queryGet, variables = new { id = producto.ShopifyVariantId } };
                var jsonContentGet = new StringContent(System.Text.Json.JsonSerializer.Serialize(payloadGet), System.Text.Encoding.UTF8, "application/json");
                var responseGet = await _httpClient.PostAsync("", jsonContentGet);
                var jsonResponseGet = await responseGet.Content.ReadAsStringAsync();

                var jsonNodeGet = System.Text.Json.Nodes.JsonNode.Parse(jsonResponseGet);
                var productId = jsonNodeGet["data"]?["productVariant"]?["product"]?["id"]?.ToString();

                if (string.IsNullOrEmpty(productId))
                {
                    Console.WriteLine($"[ERROR] No se encontró el Producto Padre para {producto.Sku}");
                    return;
                }

                // PASO 2: Actualizar usando la NUEVA mutación obligatoria de Shopify
                var mutationUpdate = @"
            mutation actualizarPrecio($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
              productVariantsBulkUpdate(productId: $productId, variants: $variants) {
                userErrors {
                  field
                  message
                }
              }
            }";

                var variablesUpdate = new
                {
                    productId = productId,
                    variants = new[] {
                new {
                    id = producto.ShopifyVariantId,
                    // Mantenemos la cultura invariante para evitar el problema de la coma decimal
                    price = producto.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }
            }
                };

                var payloadUpdate = new { query = mutationUpdate, variables = variablesUpdate };
                var jsonContentUpdate = new StringContent(System.Text.Json.JsonSerializer.Serialize(payloadUpdate), System.Text.Encoding.UTF8, "application/json");

                var responseUpdate = await _httpClient.PostAsync("", jsonContentUpdate);
                var jsonResponseUpdate = await responseUpdate.Content.ReadAsStringAsync();

                var jsonNodeUpdate = System.Text.Json.Nodes.JsonNode.Parse(jsonResponseUpdate);
                var userErrors = jsonNodeUpdate["data"]?["productVariantsBulkUpdate"]?["userErrors"]?.AsArray();

                if (userErrors != null && userErrors.Count > 0)
                {
                    Console.WriteLine($"[ERROR SHOPIFY] Falló {producto.Sku}. Razón: {userErrors[0]["message"]}");
                }
                else
                {
                    Console.WriteLine($"[ÉXITO] Precio de {producto.Sku} actualizado correctamente a {producto.Price}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR C#] Excepción en {producto.Sku}: {ex.Message}");
            }
        }

        public async Task<List<HomologacionDto>> ExtraerDiccionarioIdsAsync()
        {
            var diccionario = new List<HomologacionDto>();
            bool hasNextPage = true;
            string cursor = null;

            Console.WriteLine("Iniciando Bootstrapping: Descargando diccionario de Shopify...");

            while (hasNextPage)
            {
                // La consulta GraphQL con soporte para paginación
                var query = @"
            query obtenerVariantes($cursor: String) {
              productVariants(first: 250, after: $cursor) {
                pageInfo {
                  hasNextPage
                  endCursor
                }
                edges {
                  node {
                    id
                    sku
                  }
                }
              }
            }";

                var payload = new { query = query, variables = new { cursor = cursor } };
                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("", jsonContent);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserializamos usando nuestras nuevas clases
                var shopifyData = JsonSerializer.Deserialize<ShopifyGraphQLResponse>(jsonResponse);
                var variantes = shopifyData?.Data?.ProductVariants;

                if (variantes != null)
                {
                    // Filtramos solo los que tengan SKU configurado en Shopify
                    foreach (var edge in variantes.Edges.Where(e => !string.IsNullOrEmpty(e.Node.Sku)))
                    {
                        diccionario.Add(new HomologacionDto
                        {
                            Sku = edge.Node.Sku,
                            ShopifyVariantId = edge.Node.Id
                        });
                    }

                    // Actualizamos las variables para la siguiente vuelta del bucle
                    hasNextPage = variantes.PageInfo.HasNextPage;
                    cursor = variantes.PageInfo.EndCursor;
                }
                else
                {
                    hasNextPage = false;
                }
            }

            Console.WriteLine($"Descarga completa. Se encontraron {diccionario.Count} SKUs homologables.");
            return diccionario;
        }

        public async Task<(string ProductId, string VariantId)> CrearProductoAsync(ShopifyUpdateRequest producto)
        {
            Console.WriteLine($"[SHOPIFY] Creando producto nuevo: {producto.Sku}...");

            var mutation = @"
        mutation crearProductoNuevo($input: ProductSetInput!) {
          productSet(synchronous: true, input: $input) {
            product {
              id   # <--- AHORA PEDIMOS EL ID DEL PADRE AQUÍ
              variants(first: 1) {
                edges {
                  node {
                    id
                  }
                }
              }
            }
            userErrors { 
              field 
              message 
            }
          }
        }";

            var variables = new
            {
                input = new
                {
                    title = producto.Titulo,
                    status = "ACTIVE",
                    productOptions = new[] {
                new {
                    name = "Title",
                    values = new[] { new { name = "Default Title" } }
                }
            },
                    variants = new[] {
                new {
                    sku = producto.Sku,
                    price = producto.Price.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    optionValues = new[] {
                        new { optionName = "Title", name = "Default Title" }
                    }
                }
            }
                }
            };

            var payload = new { query = mutation, variables = variables };
            var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("", jsonContent);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonNode = System.Text.Json.Nodes.JsonNode.Parse(jsonResponse);

                var userErrors = jsonNode["data"]?["productSet"]?["userErrors"]?.AsArray();
                if (userErrors != null && userErrors.Count > 0)
                {
                    Console.WriteLine($"[ERROR SHOPIFY] No se pudo crear {producto.Sku}. Razón: {userErrors[0]["message"]}");
                    return (null, null); // Retornamos la tupla vacía
                }

                // Extraemos AMBOS IDs del JSON
                string productId = jsonNode["data"]?["productSet"]?["product"]?["id"]?.ToString();
                string variantId = jsonNode["data"]?["productSet"]?["product"]?["variants"]?["edges"]?[0]?["node"]?["id"]?.ToString();

                return (productId, variantId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR C#] Falló la lectura del JSON para {producto.Sku}: {ex.Message}");
                return (null, null);
            }
        }
    }
}