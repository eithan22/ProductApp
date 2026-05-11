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
      
        public string Nombre { get; private set; } = string.Empty;
        public string Descripcion { get; private set; } = string.Empty;
        public decimal Precio { get; private set; }
        
        public decimal Costo { get; private set; }
        public EstadoProducto Estado { get; private set; }= EstadoProducto.Disponible;

        public Inventario Inventario { get; private set; } = null!;

        public Categoria Categoria { get; private set; } = null!;

        public int CategoriaId { get; private set; }


        public Producto(string nombre, string descripcion, decimal precio, decimal costo , int categoriaId)
        {
            CambiarYvalidarNombre(nombre);
            CambiarYvalidarDescripcion(descripcion);
            CambiarYvalidarPrecio(precio);
            CambiarYvalidarCosto(costo);
            CambiarYvalidarCategoria(categoriaId);

                Nombre = nombre;
                Descripcion = descripcion;
                Precio = precio;
                Costo = costo;
                CategoriaId = categoriaId;

                Estado = EstadoProducto.Disponible;
            
            

        }









        public void CambiarYvalidarNombre(string nombre)
        {
            if(string.IsNullOrWhiteSpace(nombre))
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
                      if(string.IsNullOrWhiteSpace(descripcion))
            {
                throw new ArgumentException("La descripcion no puede estar vacia.");
            }
            Descripcion = descripcion;
        }




        public void CambiarYvalidarCosto(decimal costo)
        {
            if(costo < 0)
            {
                throw new ArgumentException("El costo no puede ser negativo.");
            }
            Costo = costo;
        }

        public void CambiarYvalidarCategoria(int categoriaId)
        {
            if(categoriaId < 0)
            {
                throw new ArgumentException("El id de la categoria no puede ser 0");

            }
            CategoriaId = categoriaId;

        }


        public void DesactivarProducto()
        {
            if(Estado == EstadoProducto.Inactivo)
            {
                throw new InvalidOperationException("El producto ya está inactivo.");
            }

            Estado = EstadoProducto.Inactivo;
        }

        public void ActivarProducto()
        {
            if(Estado == EstadoProducto.Activo)
            {
                throw new InvalidOperationException("El producto ya está activo.");
            }
            Estado = EstadoProducto.Activo;
        }


    }
}
