namespace ProductApp.Domian.Common.Exceptions
{
    public class ValidacionDominioException : DomainException
    {
        public string Campo { get; }

        public ValidacionDominioException(string mensaje) : base(mensaje)
        {
            Campo = string.Empty;
        }

        public ValidacionDominioException(string campo, string mensaje) : base(mensaje)
        {
            Campo = campo;
        }
    }
}
