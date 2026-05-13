using ProductApp.Domian.Common.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ProductApp.Domian.Entitis
{
    public class Inventario : BaseEntity
    {
        public int CantidadActual { get;private set; }

       public int CantidadMinima { get; private set; }

       
       public int ProductoId { get; private set; }

      public DateTime UltimaActualizacion { get; private set; } = DateTime.Now;

      public Producto Producto { get; set; } = null!;

        public Inventario(int cantidadActual, int cantidadMinima, int productoId)
          {

            if (cantidadActual < 0)
            {
                throw new ArgumentException("La cantidad actual no puede ser negativa.");
            }

            if (cantidadMinima < 0)
            {
                throw new ArgumentException("La cantidad mínima no puede ser negativa.");
            }

            CantidadActual = cantidadActual;
                CantidadMinima = cantidadMinima;
                ProductoId = productoId;

            UltimaActualizacion = DateTime.Now;




        }


        public void AjustarStock(int nuevoStock)
        {
            if (nuevoStock < 0)
            {
                throw new ArgumentException("El stock no puede ser negativo.");
            }

            CantidadActual = nuevoStock;
            UltimaActualizacion = DateTime.Now;
        }





        public void RegistrarEntradaStock(int cantidad)
        {
            if(cantidad < 0)
            {
                throw new ArgumentException("La cantidad de movimiento no puede ser negativa.");
            };

            CantidadActual += cantidad;
            UltimaActualizacion = DateTime.Now;
        }

        public void RegistrarSalidaStock(int cantidad)
        {
            if (cantidad <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor a 0.");
            }

            if (cantidad > CantidadActual)
            {
                throw new InvalidOperationException("No hay suficiente stock.");
            }

            CantidadActual -= cantidad;
            UltimaActualizacion = DateTime.Now;
        }




    }
}
