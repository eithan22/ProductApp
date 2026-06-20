
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Domian.Entitis;


namespace ProductApp.Aplication.Mappers.Modulo_Ventas
{
    public class OrdenDetalleMapper : IMapperDetalleOrden
    {
        

        public OrdenDetalle MapToCreateDetalleOrden(CreateDetalleOrdenDto dto, Producto producto)
        {
           return new OrdenDetalle
                (
                   dto.ProductId,
                    dto.Cantidad,
                    producto.Precio,
                    dto.OrdenId
                );  
        }

        public OrdenDetalleResponseDto MapToDetalleOrdenResponseDto(OrdenDetalle detalle)
        {
            var response = new OrdenDetalleResponseDto()
            {
                ID = detalle.Id,
                Producto = detalle.Producto.Nombre,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Subtotal = detalle.Subtotal,
                OrdenId = detalle.OrdenId
            };
            return response;
        }

        public void MapToUpdateDetalleOrden(UpdateDetalleOrdenDto dto, OrdenDetalle detalle)
        {
            detalle.ActualizarCantidad(dto.Cantidad);

        }
    }
}
