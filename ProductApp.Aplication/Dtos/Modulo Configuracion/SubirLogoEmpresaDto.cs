namespace ProductApp.Aplication.Dtos.Modulo_Configuracion
{
    public class SubirLogoEmpresaDto
    {
        public Stream Contenido { get; set; } = null!;
        public string NombreArchivo { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long TamanoBytes { get; set; }
    }
}
