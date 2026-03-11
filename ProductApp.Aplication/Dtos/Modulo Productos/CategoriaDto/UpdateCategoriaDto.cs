using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.CategoriaDto
{
    public class UpdateCategoriaDto
    {

            public int Id { get; set; }
            public string Nombre { get; set; } = null!;
            public string Descripcion { get; set; } = null!;
    }
}
