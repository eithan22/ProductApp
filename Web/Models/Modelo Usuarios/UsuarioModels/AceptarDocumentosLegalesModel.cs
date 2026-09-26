namespace Web.Models.Modelo_Usuarios.UsuarioModels
{
    public class AceptarDocumentosLegalesModel
    {
        // Viaja en un campo oculto: es la versión que el usuario tenía en pantalla. La API la
        // compara con la vigente y rechaza la aceptación si cambió mientras leía. Que el campo
        // sea manipulable desde el cliente no abre nada: mandar otra versión hace que la API
        // rechace, no que acepte.
        public string Version { get; set; } = string.Empty;

        public bool Acepto { get; set; }
    }
}
