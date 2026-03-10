using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Common.Exceptions.ExceptionsProducto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Entitis
{
    public class Producto : BaseEntity
    {
      
        public string Nombre { get;  set; } = string.Empty;
        public string Descripcion { get;  set; } = string.Empty;
        public decimal Precio { get;  set; }
        
        public decimal Costo { get;  set; }
        public EstadoProducto Estado { get; set; }= EstadoProducto.Disponible;

        public Inventario Inventario { get; set; } = null!;

        public Categoria Categoria { get; set; } = null!;

        public int CategoriaId { get; set; }




      


        public void CambiarYvalidarNombre(string nombre)
        {
            if(string.IsNullOrWhiteSpace(Nombre))
            {
                throw new ArgumentException("El nombre no puede estar vacio.");
            }
            Nombre = nombre;
        }


        public void CambiarYvalidarPrecio(decimal precio)
        {
            if(precio < 0)
            {
                throw new PrecioInvalidoException(precio); //ejemplo de exceptions
            }
            Precio = precio;
        }

    
       

        //apernder hacer estops metodos de validacion
        public void CambiarYvalidarDescripcion(string descripcion) {
                      if(string.IsNullOrWhiteSpace(Descripcion))
            {
                throw new ArgumentException("La descripcion no puede estar vacia.");
            }
            Descripcion = descripcion;
        }




    }
}
