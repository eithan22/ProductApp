namespace ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto
{
    public class FilaErrorImportacionDto
    {
        public int NumeroFila { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Columna { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }
}
