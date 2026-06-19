namespace ProductApp.Domian.Common.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string mensaje) : base(mensaje) { }
    }
}
