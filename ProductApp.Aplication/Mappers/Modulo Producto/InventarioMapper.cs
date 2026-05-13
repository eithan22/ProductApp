using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Mappers.Modulo_Producto
{
    public class InventarioMapper : IMapperInventario
    {
        public InventarioResponseDto MapToInventarioResponse(Inventario inventario)
        {
           var responseDto = new InventarioResponseDto
            {
                Id = inventario.Id,
                Producto = inventario.Producto.Nombre,
                StockActual = inventario.CantidadActual,
                StockMinimo = inventario.CantidadMinima,
                FechaActualizacion = inventario.UltimaActualizacion
            };
            return responseDto;
        }
    }
}
