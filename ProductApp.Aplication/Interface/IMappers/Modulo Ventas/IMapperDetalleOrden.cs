using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Ventas
{
    public interface IMapperDetalleOrden
    {
        OrdenDetalle MapToCreateDetalleOrden(CreateDetalleOrdenDto dto , Producto producto);
         OrdenDetalleResponseDto MapToDetalleOrdenResponseDto(OrdenDetalle detalle);
          void MapToUpdateDetalleOrden(UpdateDetalleOrdenDto dto, OrdenDetalle detalle);
    }
}
