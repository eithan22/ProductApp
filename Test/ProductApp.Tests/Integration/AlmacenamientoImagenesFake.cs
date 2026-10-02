using ProductApp.Domian.Interfaces;

namespace ProductApp.Tests.Integration
{
    internal class AlmacenamientoImagenesFake : IAlmacenamientoImagenes
    {
        public string? UltimoNombreArchivo { get; private set; }
        public string? UltimoContentType { get; private set; }

        public Task<string> SubirAsync(Stream contenido, string nombreArchivo, string contentType, CancellationToken cancellationToken = default)
        {
            UltimoNombreArchivo = nombreArchivo;
            UltimoContentType = contentType;
            return Task.FromResult($"https://fake-blob/imagenes/{nombreArchivo}");
        }

        public Task EliminarAsync(string imagenUrl, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public string ObtenerUrlConSas(string imagenUrl) => imagenUrl;
    }
}
