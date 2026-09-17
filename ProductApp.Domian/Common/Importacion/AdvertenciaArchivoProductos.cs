namespace ProductApp.Domian.Common.Importacion
{
    // Aviso no bloqueante: la fila SÍ se creó, pero un dato se interpretó de forma distinta
    // a como venía escrito (ej. "electronica" resuelto como la categoría "Electrónica", o un
    // proveedor que no se encontró y el producto quedó sin proveedor asignado).
    public sealed class AdvertenciaArchivoProductos
    {
        public int NumeroFila { get; init; }
        public string Columna { get; init; } = string.Empty;
        public string Mensaje { get; init; } = string.Empty;
    }
}
