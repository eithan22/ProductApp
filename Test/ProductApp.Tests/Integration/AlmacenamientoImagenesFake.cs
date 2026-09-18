using ProductApp.Domian.Interfaces;

namespace ProductApp.Tests.Integration
{
    internal class AlmacenamientoImagenesFake : IAlmacenamientoImagenes
    {
        public Task<string> SubirAsync(Stream contenido, string nombreArchivo, string contentType, CancellationToken cancellationToken = default)
            => Task.FromResult($"https://fake-blob/imagenes/{nombreArchivo}");

        public Task EliminarAsync(string imagenUrl, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
