namespace ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto
{
    // La versión que el usuario tenía en pantalla cuando marcó la casilla. El cliente la manda
    // de vuelta para que el servidor pueda rechazar la aceptación de un texto viejo: si se
    // publicó una versión nueva mientras el usuario leía, lo que aceptó ya no es lo que rige.
    public class AceptarDocumentosLegalesDto
    {
        public string Version { get; set; } = string.Empty;
    }
}
