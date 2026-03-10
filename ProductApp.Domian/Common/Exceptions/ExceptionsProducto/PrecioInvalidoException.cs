using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Common.Exceptions.ExceptionsProducto
{
    public class PrecioInvalidoException : Exception
    {
        public PrecioInvalidoException(decimal Precio)

           : base($"El precio {Precio} no puede ser negativo.")
        {
        }




    }

   
}

