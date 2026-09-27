namespace ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto
{
    // Alimenta la pantalla de resultado (ProductosImportarResultado): los cuatro contadores
    // de arriba, la lista de errores, la de omitidas por duplicado y el archivo descargable.
    public class ImportacionProductosResultadoDto
    {
        public string NombreArchivo { get; set; } = string.Empty;

        public int TotalFilas { get; set; }
        public int FilasCreadas { get; set; }
        public int FilasConError { get; set; }
        public int FilasOmitidas { get; set; }

        public List<FilaErrorImportacionDto> Errores { get; set; } = new();
        public List<FilaDuplicadaImportacionDto> Duplicadas { get; set; } = new();
        public List<AdvertenciaImportacionDto> Advertencias { get; set; } = new();

        // El .xlsx de filas rechazadas viaja en la misma respuesta en base64 en vez de
        // persistirse: la importación no deja archivos huérfanos en el storage y la Web solo
        // tiene que decodificarlo cuando el usuario toca "Descargar filas con error".
        public string? ArchivoErroresBase64 { get; set; }
        public string? NombreArchivoErrores { get; set; }
    }
}
