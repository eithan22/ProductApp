namespace ProductApp.Domian.Interfaces
{
    // Puerto hermano de IAlmacenamientoFacturas: el dominio declara que necesita agrupar
    // varias escrituras en una sola unidad atómica, sin saber que detrás hay EF Core.
    public interface IGestorTransacciones
    {
        // Transacción serializable: además de atomicidad, impide que dos peticiones
        // simultáneas lean el mismo saldo y ambas den por válido el mismo cobro.
        Task<ITransaccionActiva> IniciarSerializableAsync();

        // Quien conoce los códigos de error del motor es la infraestructura, no el servicio.
        // Esto le permite a la capa de aplicación distinguir "chocaron dos peticiones"
        // (reintentable, mensaje de negocio) de un error real, sin depender de tipos de EF Core.
        bool EsConflictoDeConcurrencia(Exception ex);
    }

    public interface ITransaccionActiva : IAsyncDisposable
    {
        Task CommitAsync();

        Task RollbackAsync();
    }
}
