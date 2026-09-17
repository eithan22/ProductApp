namespace Web.Models.Modelo_Productos.ImportacionModels
{
    // Espejo de ImportacionProductosResultadoDto. Los nombres tienen que coincidir
    // con los del DTO: BaseHttpServices deserializa por nombre (case-insensitive).
    public class ImportacionProductosResultadoModel
    {
        public string NombreArchivo { get; set; } = string.Empty;

        public int TotalFilas { get; set; }
        public int FilasCreadas { get; set; }
        public int FilasConError { get; set; }
        public int FilasOmitidas { get; set; }

        public List<FilaErrorImportacionModel> Errores { get; set; } = new();
        public List<FilaDuplicadaImportacionModel> Duplicadas { get; set; } = new();
        public List<AdvertenciaImportacionModel> Advertencias { get; set; } = new();

        // El .xlsx de filas rechazadas llega acá en base64 dentro de la misma respuesta:
        // no hay un segundo endpoint que pedirle a la API. La vista lo reenvía al
        // controller para que la descarga salga con File(...) como todas las demás.
        public string? ArchivoErroresBase64 { get; set; }
        public string? NombreArchivoErrores { get; set; }
    }
}
