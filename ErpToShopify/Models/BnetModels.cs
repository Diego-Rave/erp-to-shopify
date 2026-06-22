using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ErpToShopify.Models 
{
    public class BnetApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public List<BnetProductoDto> Data { get; set; }
    }

    public class BnetProductoDto
    {
        [JsonPropertyName("codigo_sku")]
        public string Sku { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("precio_venta")]
        public decimal PrecioVenta { get; set; }

        [JsonPropertyName("saldo_disponible")]
        public int SaldoDisponible { get; set; }

        [JsonPropertyName("activo")]
        public bool Activo { get; set; }

        [JsonPropertyName("homologacion")]
        public BnetHomologacionDto Homologacion { get; set; }
    }

    public class BnetHomologacionDto
    {
        [JsonPropertyName("shopify_product_id")]
        public string ShopifyProductId { get; set; }

        [JsonPropertyName("shopify_variant_id")]
        public string ShopifyVariantId { get; set; }
    }
}