using Microsoft.EntityFrameworkCore.Storage.Json;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Mappers.Modulo_Producto
{
    public class ProductoMapper : IMapperProducto
    {
        public Producto MapToCreateProducto(CreateProductoDto dto)
        {
            var producto = new Producto(
                dto.Nombre,
                dto.Descripcion,
                dto.Precio,
                dto.Costo,
                dto.CategoriaId

                );

            if (!string.IsNullOrWhiteSpace(dto.ImagenUrl))
                producto.AsignarImagen(dto.ImagenUrl);

            if (dto.ProveedorId.HasValue)
                producto.AsignarProveedor(dto.ProveedorId);

            return producto;

        }

        public ProductoResponseDto MapToProductoResponse(Producto producto)
        {

            var productoResponse = new ProductoResponseDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Costo = producto.Costo,
                Estado = producto.Estado.ToString(),
                Categoria = producto.Categoria?.Nombre,
                ImagenUrl = producto.ImagenUrl,
                ProveedorId = producto.ProveedorId,
                Proveedor = producto.Proveedor?.Nombre,
                StockActual = producto.Inventario?.CantidadActual,
                StockMinimo = producto.Inventario?.CantidadMinima,

            };

            return productoResponse;
                                      
        }

        public void MapToUpdateProducto(UpdateProductoDto dto, Producto producto)
        {
            producto.CambiarYvalidarPrecio(dto.Precio);
            producto.CambiarYvalidarCosto(dto.Costo);
            producto.CambiarYvalidarDescripcion(dto.Descripcion);
            producto.CambiarYvalidarNombre(dto.Nombre);
            producto.CambiarYvalidarCategoria(dto.CategoriaId);

            // El proveedor sí se aplica tal cual venga, incluido null: así el formulario
            // puede quitarle el proveedor a un producto que ya lo tenía.
            producto.AsignarProveedor(dto.ProveedorId);

            // Si el dto no trae imagen se conserva la que ya tenía: el update por JSON nunca
            // borra un archivo ya subido al storage.
            if (!string.IsNullOrWhiteSpace(dto.ImagenUrl))
                producto.AsignarImagen(dto.ImagenUrl);


        }
    }
}
