using ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Busqueda;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Mappers.Modulo_Busqueda
{
    public class BusquedaMapper : IMapperBusqueda
    {
        public BusquedaProductoItemDto MapToProductoItem(Producto producto)
        {
            return new BusquedaProductoItemDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Categoria = producto.Categoria?.Nombre,
                Precio = producto.Precio,
                StockActual = producto.Inventario?.CantidadActual
            };
        }

        public BusquedaClienteItemDto MapToClienteItem(Cliente cliente)
        {
            return new BusquedaClienteItemDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Cedula = cliente.Cedula,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono
            };
        }

        public BusquedaOrdenItemDto MapToOrdenItem(Orden orden, int cantidadProductos, decimal totalPagado)
        {
            return new BusquedaOrdenItemDto
            {
                Id = orden.Id,
                NombreCliente = orden.Cliente.Nombre,
                Fecha = orden.Fecha,
                CantidadProductos = cantidadProductos,
                Estado = orden.Estado.ToString(),
                Total = orden.Total,

                // El saldo se pide al dominio en vez de restarlo aquí: la regla de cómo se
                // calcula vive en la entidad Orden, no en el mapper.
                SaldoPendiente = orden.CalcularSaldoPendiente(totalPagado)
            };
        }

        public BusquedaProveedorItemDto MapToProveedorItem(Proveedor proveedor)
        {
            return new BusquedaProveedorItemDto
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Telefono = proveedor.Telefono,
                Correo = proveedor.Correo
            };
        }
    }
}
