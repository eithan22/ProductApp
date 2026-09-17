using ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Busqueda
{
    public interface IMapperBusqueda
    {
        BusquedaProductoItemDto MapToProductoItem(Producto producto);
        BusquedaClienteItemDto MapToClienteItem(Cliente cliente);
        BusquedaOrdenItemDto MapToOrdenItem(Orden orden, int cantidadProductos, decimal totalPagado);
        BusquedaProveedorItemDto MapToProveedorItem(Proveedor proveedor);
    }
}
