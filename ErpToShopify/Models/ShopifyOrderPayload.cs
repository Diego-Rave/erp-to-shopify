using System.Text.Json.Serialization;

namespace ErpToShopify.Models
{
    // Representa el pedido general
    public class ShopifyOrderPayload
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("order_number")]
        public int OrderNumber { get; set; }

        [JsonPropertyName("total_price")]
        public string TotalPrice { get; set; }

        [JsonPropertyName("line_items")]
        public List<ShopifyLineItem> LineItems { get; set; }
    }

    // Representa cada producto dentro del pedido
    public class ShopifyLineItem
    {
        [JsonPropertyName("sku")]
        public string Sku { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }
    }
}