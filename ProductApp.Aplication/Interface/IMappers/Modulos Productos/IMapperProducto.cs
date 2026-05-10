using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.IMappers.Modulos_Productos
{
    public interface IMapperProducto
    {

        Producto MapToCreateProducto (CreateProductoDto dto);

        ProductoResponseDto MapToProductoResponse(Producto producto);

        void MapToUpdateProducto(UpdateProductoDto dto , Producto producto);




     }
}
