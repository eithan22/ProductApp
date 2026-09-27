using ProductApp.Domian.Interfaces;

namespace ProductApp.Tests.Integration
{
    // El proveedor en memoria de EF Core no soporta transacciones ni niveles de aislamiento.
    // Este doble deja que PagoService corra su flujo completo tal cual, sin transacción real:
    // los tests existentes siguen comprobando reglas de negocio, no atomicidad. La atomicidad
    // y la serialización solo son verificables contra SQL Server real.
    internal class GestorTransaccionesFake : IGestorTransacciones
    {
        // Contadores para los tests de M3/M12. Como el proveedor en memoria no revierte nada, lo
        // único verificable acá es el protocolo: cuántas transacciones se abrieron, si hubo
        // commit y si se liberaron. El rollback real sigue siendo cosa de SQL Server.
        public int TransaccionesIniciadas { get; private set; }
        public int CommitsConfirmados { get; private set; }
        public int TransaccionesLiberadas { get; private set; }

        public Task<ITransaccionActiva> IniciarSerializableAsync()
        {
            TransaccionesIniciadas++;
            return Task.FromResult<ITransaccionActiva>(new TransaccionFake(this));
        }

        public bool EsConflictoDeConcurrencia(Exception ex) => false;

        private sealed class TransaccionFake : ITransaccionActiva
        {
            private readonly GestorTransaccionesFake _gestor;

            public TransaccionFake(GestorTransaccionesFake gestor) => _gestor = gestor;

            public Task CommitAsync()
            {
                _gestor.CommitsConfirmados++;
                return Task.CompletedTask;
            }

            public Task RollbackAsync() => Task.CompletedTask;

            public ValueTask DisposeAsync()
            {
                _gestor.TransaccionesLiberadas++;
                return ValueTask.CompletedTask;
            }
        }
    }
}
