namespace ProductApp.Domian.Common.Exceptions.ExceptionsProducto
{
    public class PrecioInvalidoException : ValidacionDominioException
    {
        public PrecioInvalidoException(decimal precio)
            : base("Precio", $"El precio '{precio}' no puede ser negativo.") { }
    }
}

