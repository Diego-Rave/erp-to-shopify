using BnetShopifyIntegrator.Models;
using System.Text;


namespace BnetShopifyIntegrator.Interfaces
{
    public interface IShopifyService
    {
        Task<bool> ActualizarInventarioYPrecioAsync(List<ShopifyUpdateRequest> productos);
        Task<List<HomologacionDto>> ExtraerDiccionarioIdsAsync();

        // Devuelve el nuevo ID que Shopify genere
        Task<(string ProductId, string VariantId)> CrearProductoAsync(ShopifyUpdateRequest producto);
    }
}
