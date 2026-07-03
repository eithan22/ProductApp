using ProductApp.Aplication.Dtos.CategoriaDto;
using Web.Models.Modelo_Productos.CategoriaModels;

namespace Web.Services.Mappers.Modulo_Productos
{
    public class CategoriaMapperM
    {
        public static CreateCategoriaDto MapAddCategoriaDto(CreateCategoriaModel model)
        {
            return new CreateCategoriaDto
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion
            };
        }

        public static UpdateCategoriaDto MapUpdateCategoriaDto(UpdateCategoriaModel model)
        {
            return new UpdateCategoriaDto
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion
            };
        }
    }
}
