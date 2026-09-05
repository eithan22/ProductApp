namespace ProductApp.Domian.Interfaces
{
    public interface IAlmacenamientoImagenes
    {
        Task<string> SubirAsync(Stream contenido, string nombreArchivo, string contentType, CancellationToken cancellationToken = default);

        Task EliminarAsync(string imagenUrl, CancellationToken cancellationToken = default);
    }
}
