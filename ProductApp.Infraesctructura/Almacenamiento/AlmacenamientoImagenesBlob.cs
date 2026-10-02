using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Infraesctructura.Persistencia.Almacenamiento
{
    public class AlmacenamientoImagenesBlob : IAlmacenamientoImagenes
    {
        private static readonly TimeSpan DuracionSas = TimeSpan.FromMinutes(15);

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

        public string ObtenerUrlConSas(string imagenUrl)
        {
            if (string.IsNullOrWhiteSpace(imagenUrl))
                return imagenUrl;

            if (!Uri.TryCreate(imagenUrl, UriKind.Absolute, out var uri))
                return imagenUrl;

            var nombreBlob = new BlobUriBuilder(uri).BlobName;
            if (string.IsNullOrWhiteSpace(nombreBlob))
                return imagenUrl;

            var blob = _contenedor.GetBlobClient(nombreBlob);

            if (!blob.CanGenerateSasUri)
                return imagenUrl;

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _contenedor.Name,
                BlobName = nombreBlob,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(DuracionSas)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blob.GenerateSasUri(sasBuilder).ToString();
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

                await _contenedor.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

                // El contenedor puede existir de antes con acceso público (versión previa de
                // este código, que lo creaba con PublicAccessType.Blob) — CreateIfNotExistsAsync
                // no toca el nivel de acceso de un contenedor ya existente, así que hay que
                // forzarlo a privado explícitamente en cada arranque, no solo al crearlo.
                await _contenedor.SetAccessPolicyAsync(PublicAccessType.None, cancellationToken: cancellationToken);

                _contenedorVerificado = true;
            }
            finally
            {
                _semaforoContenedor.Release();
            }
        }
    }
}
