using System.Text.Json.Serialization;

namespace ErpToShopify.Models
{
    // Este es el modelo final que enviaremos a Dapper
    public class HomologacionDto
    {
        public string Sku { get; set; }

        [JsonPropertyName("shopify_product_id")]
        public string ShopifyProductId { get; set; }

        [JsonPropertyName("shopify_variant_id")]
        public string ShopifyVariantId { get; set; }
    }

    // --- Clases para deserializar el JSON anidado de Shopify ---
    public class ShopifyGraphQLResponse
    {
        [JsonPropertyName("data")]
        public ShopifyData Data { get; set; }
    }

    public class ShopifyData
    {
        [JsonPropertyName("productVariants")]
        public ShopifyProductVariants ProductVariants { get; set; }
    }

    public class ShopifyProductVariants
    {
        [JsonPropertyName("pageInfo")]
        public ShopifyPageInfo PageInfo { get; set; }

        [JsonPropertyName("edges")]
        public List<ShopifyEdge> Edges { get; set; }
    }

    public class ShopifyPageInfo
    {
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonPropertyName("endCursor")]
        public string EndCursor { get; set; }
    }

    public class ShopifyEdge
    {
        [JsonPropertyName("node")]
        public ShopifyNode Node { get; set; }
    }

    public class ShopifyNode
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("sku")]
        public string Sku { get; set; }
    }
}