namespace ProductApp.Domian.Common.Importacion
{
    // Resultado de abrir y parsear el archivo. No usa OperationResult porque ese tipo vive
    // en Application y Domain no puede depender de él.
    public sealed class ResultadoLecturaArchivoProductos
    {
        public bool FormatoSoportado { get; private init; }
        public IReadOnlyList<string> ColumnasFaltantes { get; private init; } = Array.Empty<string>();
        public IReadOnlyList<FilaArchivoProductos> Filas { get; private init; } = Array.Empty<FilaArchivoProductos>();

        public static ResultadoLecturaArchivoProductos FormatoNoSoportado() =>
            new() { FormatoSoportado = false };

        public static ResultadoLecturaArchivoProductos FaltanColumnas(IReadOnlyList<string> columnasFaltantes) =>
            new() { FormatoSoportado = true, ColumnasFaltantes = columnasFaltantes };

        public static ResultadoLecturaArchivoProductos Leido(IReadOnlyList<FilaArchivoProductos> filas) =>
            new() { FormatoSoportado = true, Filas = filas };
    }
}
