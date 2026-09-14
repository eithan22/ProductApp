using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Infraesctructura.Persistencia.Almacenamiento
{
    public class AlmacenamientoFacturasBlob : IAlmacenamientoFacturas
    {
        private const string ContentTypePdf = "application/pdf";

        private readonly BlobContainerClient _contenedor;
        private readonly SemaphoreSlim _semaforoContenedor = new(1, 1);
        private bool _contenedorVerificado;

        public AlmacenamientoFacturasBlob(BlobServiceClient blobServiceClient, string nombreContenedor)
        {
            _contenedor = blobServiceClient.GetBlobContainerClient(nombreContenedor);
        }

        public async Task<string> SubirAsync(byte[] contenido, string nombreBlob, CancellationToken cancellationToken = default)
        {
            await AsegurarContenedorAsync(cancellationToken);

            var blob = _contenedor.GetBlobClient(nombreBlob);

            using var memoria = new MemoryStream(contenido, writable: false);

            // Sobrescribe si ya existía: el nombre del blob es determinístico por orden
            // (orden-{id}.pdf), así que regenerar una factura reemplaza la anterior.
            await blob.UploadAsync(
                memoria,
                new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = ContentTypePdf } },
                cancellationToken);

            return blob.Uri.ToString();
        }

        public async Task<byte[]?> DescargarAsync(string nombreBlob, CancellationToken cancellationToken = default)
        {
            await AsegurarContenedorAsync(cancellationToken);

            var blob = _contenedor.GetBlobClient(nombreBlob);

            var existe = await blob.ExistsAsync(cancellationToken);
            if (!existe.Value)
                return null;

            using var memoria = new MemoryStream();
            await blob.DownloadToAsync(memoria, cancellationToken);

            return memoria.ToArray();
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

                // Contenedor privado (sin PublicAccessType): a diferencia de las imágenes de
                // producto, una factura tiene datos del cliente y solo se entrega por la API
                // autenticada (GET Orden/GetFactura/{id}), nunca por url directa.
                await _contenedor.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

                _contenedorVerificado = true;
            }
            finally
            {
                _semaforoContenedor.Release();
            }
        }
    }
}
