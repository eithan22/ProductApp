namespace ProductApp.Domian.Common.Exceptions
{
    public class DuplicadoException : DomainException
    {
        public DuplicadoException(string mensaje) : base(mensaje) { }
    }
}
