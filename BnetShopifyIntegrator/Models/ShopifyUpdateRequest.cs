using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BnetShopifyIntegrator.Models
{
    public class ShopifyUpdateRequest
    {
        public string Sku { get; set; }
        public string Titulo { get; set; }
        public decimal Price { get; set; }
        public int InventoryQuantity { get; set; }
        public string ShopifyVariantId { get; set; }
    }
}
