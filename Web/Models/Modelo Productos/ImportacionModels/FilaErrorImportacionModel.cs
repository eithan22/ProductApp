namespace Web.Models.Modelo_Productos.ImportacionModels
{
    public class FilaErrorImportacionModel
    {
        public int NumeroFila { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Columna { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }
}
