namespace ProductApp.Domian.Common.Importacion
{
    // Una fila cruda del archivo importado. Todo se expone como texto a propósito: quien
    // decide si "RD$ 185,00" es un precio inválido es la capa de aplicación, no el lector.
    // El lector solo sabe de archivos, no de reglas de negocio.
    public sealed class FilaArchivoProductos
    {
        // Número de fila tal como lo ve el usuario en Excel (encabezados = 1, primera fila
        // de datos = 2). Es el dato que se le devuelve en el reporte de errores.
        public int NumeroFila { get; init; }

        public string? Nombre { get; init; }
        public string? Descripcion { get; init; }
        public string? Precio { get; init; }
        public string? Costo { get; init; }
        public string? Categoria { get; init; }
        public string? Proveedor { get; init; }
    }
}
