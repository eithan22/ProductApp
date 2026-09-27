using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using System.Data;

namespace ProductApp.Infraesctructura.Persistencia.Transacciones
{
    // Todos los repositorios reciben por inyección la MISMA instancia scoped de AppDbContext
    // dentro de una request (ver comentario en BusquedaService). Por eso una sola transacción
    // abierta acá cubre todos los SaveChangesAsync que hagan los repositorios mientras dure,
    // sin necesidad de un Unit of Work completo.
    //
    // Cuidado a futuro: si algún día se activa EnableRetryOnFailure en InfraestructuraExtension,
    // las transacciones manuales dejan de funcionar y hay que envolverlas en
    // Database.CreateExecutionStrategy().ExecuteAsync(...).
    public class GestorTransaccionesEfCore : IGestorTransacciones
    {
        private readonly AppDbContext _context;

        public GestorTransaccionesEfCore(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ITransaccionActiva> IniciarSerializableAsync()
            => new TransaccionEfCore(
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable));

        public bool EsConflictoDeConcurrencia(Exception ex)
        {
            // Bajo aislamiento serializable, dos peticiones simultáneas sobre la misma orden
            // terminan casi siempre en una de estas dos: una espera y luego falla la regla de
            // negocio (saldo 0), o SQL Server elige una como víctima de interbloqueo. La
            // víctima ya quedó revertida por el motor, así que es seguro responder
            // "reintente" en vez de dejar escapar un 500.
            for (Exception? actual = ex; actual is not null; actual = actual.InnerException)
            {
                if (actual is DbUpdateConcurrencyException)
                    return true;

                // 1205 interbloqueo, 1222 tiempo de espera de bloqueo agotado,
                // 3960 conflicto de actualización por instantánea.
                if (actual is SqlException sql && (sql.Number == 1205 || sql.Number == 1222 || sql.Number == 3960))
                    return true;
            }

            return false;
        }
    }

    internal sealed class TransaccionEfCore : ITransaccionActiva
    {
        private readonly IDbContextTransaction _transaccion;

        public TransaccionEfCore(IDbContextTransaction transaccion)
        {
            _transaccion = transaccion;
        }

        public Task CommitAsync() => _transaccion.CommitAsync();

        public Task RollbackAsync() => _transaccion.RollbackAsync();

        // Si se libera sin commit (return temprano o excepción), EF Core revierte
        // automáticamente al liberar la transacción. No hace falta rollback explícito.
        public ValueTask DisposeAsync() => _transaccion.DisposeAsync();
    }
}
