using BnetShopifyIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BnetShopifyIntegrator.Interfaces
{
    public interface IBnetService
    {
        Task<List<BnetProduct>> ObtenerProductosActivosAsync();

        //El contrato para guardar el pedido en BNET
        Task<bool> CrearPedidoAsync(ShopifyOrderPayload pedidoShopify);

        // El contrato para guardar ShopifyVariantId
        Task GuardarHomologacionAsync(List<HomologacionDto> diccionario);

        // Guarda el ID individual en la base de datos
        Task GuardarNuevoIdHomologadoAsync(string sku, string productId, string variantId);
    }

}
