namespace ProductApp.Domian.Common.Exceptions
{
    public class EstadoInvalidoException : DomainException
    {
        public EstadoInvalidoException(string mensaje) : base(mensaje) { }

        public EstadoInvalidoException(string entidad, string estadoActual, string operacion)
            : base($"No se puede ejecutar '{operacion}' en '{entidad}' con estado '{estadoActual}'.") { }
    }
}
