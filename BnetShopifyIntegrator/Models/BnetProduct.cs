using System.Text.Json.Serialization;

namespace BnetShopifyIntegrator.Models
{
    public class BnetProduct
    {
        [JsonPropertyName("codigo_sku")]
        public string Codigo { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("precio_venta")]
        public decimal Precio { get; set; }

        [JsonPropertyName("saldo_disponible")]
        public int SaldoBodega { get; set; }

        [JsonIgnore]
        public string NombreBodega { get; set; }

        [JsonPropertyName("homologacion")]
        public HomologacionDto Homologacion { get; set; }

        [JsonIgnore]
        public string ShopifyVariantId 
        { 
            get => Homologacion?.ShopifyVariantId; 
            set 
            { 
                if (Homologacion == null) Homologacion = new HomologacionDto();
                Homologacion.ShopifyVariantId = value; 
            }
        }

        [JsonIgnore]
        public string ShopifyProductId 
        { 
            get => Homologacion?.ShopifyProductId; 
            set 
            { 
                if (Homologacion == null) Homologacion = new HomologacionDto();
                Homologacion.ShopifyProductId = value; 
            }
        }
    }
} // <-- El archivo termina aquí, sin la clase HomologacionDto debajo