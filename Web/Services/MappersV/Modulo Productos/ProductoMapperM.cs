using ProductApp.Aplication.Dtos.ProductoDto;
using Web.Models.Modelo_Productos.ProductoModels;

namespace Web.Services.Mappers.Modulo_Productos
{
    public class ProductoMapperM
    {
        public static CreateProductoDto MapAddProductoDto(CreateProductoModel model)
        {
            return new CreateProductoDto
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Precio = model.Precio,
                Costo = model.Costo,
                CategoriaId = model.CategoriaId
            };
        }

        public static UpdateProductoDto MapUpdateProductoDto(UpdateProductoModel model)
        {
            return new UpdateProductoDto
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Precio = model.Precio,
                Costo = model.Costo,
                CategoriaId = model.CategoriaId
            };
        }
    }
}
