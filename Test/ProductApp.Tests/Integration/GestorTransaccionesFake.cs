using ProductApp.Domian.Interfaces;

namespace ProductApp.Tests.Integration
{
    // El proveedor en memoria de EF Core no soporta transacciones ni niveles de aislamiento.
    // Este doble deja que PagoService corra su flujo completo tal cual, sin transacción real:
    // los tests existentes siguen comprobando reglas de negocio, no atomicidad. La atomicidad
    // y la serialización solo son verificables contra SQL Server real.
    internal class GestorTransaccionesFake : IGestorTransacciones
    {
        public Task<ITransaccionActiva> IniciarSerializableAsync()
            => Task.FromResult<ITransaccionActiva>(new TransaccionFake());

        public bool EsConflictoDeConcurrencia(Exception ex) => false;

        private sealed class TransaccionFake : ITransaccionActiva
        {
            public Task CommitAsync() => Task.CompletedTask;

            public Task RollbackAsync() => Task.CompletedTask;

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}
