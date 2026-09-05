using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Infraesctructura.Persistencia.Almacenamiento
{
    public class AlmacenamientoImagenesBlob : IAlmacenamientoImagenes
    {
        private readonly BlobContainerClient _contenedor;
        private readonly SemaphoreSlim _semaforoContenedor = new(1, 1);
        private bool _contenedorVerificado;

        public AlmacenamientoImagenesBlob(BlobServiceClient blobServiceClient, string nombreContenedor)
        {
            _contenedor = blobServiceClient.GetBlobContainerClient(nombreContenedor);
        }

        public async Task<string> SubirAsync(Stream contenido, string nombreArchivo, string contentType, CancellationToken cancellationToken = default)
        {
            await AsegurarContenedorAsync(cancellationToken);

            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
            var nombreBlob = $"{Guid.NewGuid():N}{extension}";

            var blob = _contenedor.GetBlobClient(nombreBlob);

            await blob.UploadAsync(
                contenido,
                new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
                cancellationToken);

            return blob.Uri.ToString();
        }

        public async Task EliminarAsync(string imagenUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(imagenUrl))
                return;

            if (!Uri.TryCreate(imagenUrl, UriKind.Absolute, out var uri))
                return;

            var nombreBlob = new BlobUriBuilder(uri).BlobName;

            if (string.IsNullOrWhiteSpace(nombreBlob))
                return;

            await _contenedor.DeleteBlobIfExistsAsync(nombreBlob, cancellationToken: cancellationToken);
        }

        private async Task AsegurarContenedorAsync(CancellationToken cancellationToken)
        {
            if (_contenedorVerificado)
                return;

            await _semaforoContenedor.WaitAsync(cancellationToken);

            try
            {
                if (_contenedorVerificado)
                    return;

                try
                {
                    // Acceso público de solo lectura a nivel blob: la Web muestra las imágenes con un
                    // <img src="..."> directo, sin SAS ni proxy.
                    await _contenedor.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
                }
                catch (RequestFailedException)
                {
                    // La cuenta de storage puede tener deshabilitado el acceso público anónimo; en ese
                    // caso el contenedor se crea privado y las urls requerirán SAS para verse.
                    await _contenedor.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                }

                _contenedorVerificado = true;
            }
            finally
            {
                _semaforoContenedor.Release();
            }
        }
    }
}
