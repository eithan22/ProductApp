using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios
{
    public interface IVerificadorSesionService
    {
        // Recibe lo que viaja dentro del token (id y nombre del rol) y responde si esa sesión
        // sigue siendo válida contra el estado actual de la base.
        Task<OperationResult> VerificarSesionAsync(int usuarioId, string? rolEnElToken);
    }
}
