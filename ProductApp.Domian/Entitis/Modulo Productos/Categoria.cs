using ProductApp.Domian.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Entitis
{
    public class Categoria : BaseEntity
    {


        public string Nombre { get; private set; } = string.Empty;
        public string Descripcion { get; private set; } = string.Empty;
        

        public List<Producto> Productos { get; private set; } = new List<Producto>();

        public Categoria(string nombre, string descripcion)
        {
            CambiarYvalidarNombre(nombre);
            CambiarYvalidarDescripcion(descripcion);

            Nombre = nombre;
            Descripcion = descripcion;
           

        }


        public void CambiarYvalidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");
            Nombre = nombre;
        }

        public void CambiarYvalidarDescripcion(string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción no puede estar vacía.");
            Descripcion = descripcion;
        }

       

    }
}
