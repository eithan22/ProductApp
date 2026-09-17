namespace ProductApp.Domian.Common.Importacion
{
    // Fila rechazada. Lleva la fila original completa para que el reporte descargable sea
    // re-subible tal cual: el usuario corrige el motivo y vuelve a importar el mismo archivo.
    public sealed class FilaErroneaArchivoProductos
    {
        public FilaArchivoProductos Fila { get; init; } = null!;

        // Columna que causó el rechazo ("Nombre", "Precio", "Categoria"...). Cuando el fallo
        // no es de una columna puntual sino de una regla de negocio, va "General".
        public string Columna { get; init; } = string.Empty;

        public string Motivo { get; init; } = string.Empty;
    }
}
