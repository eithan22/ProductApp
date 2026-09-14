using Azure;
using Azure.Storage.Blobs;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace ProductApp.Infraesctructura.Persistencia.Facturacion
{
    public class GeneradorFacturaPdfQuestPdf : IGeneradorFacturaPdf
    {
        private readonly BlobServiceClient _blobServiceClient;

        public GeneradorFacturaPdfQuestPdf(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<byte[]> GenerarAsync(
            Orden orden,
            IReadOnlyList<OrdenDetalle> detalles,
            ConfiguracionSistema? configuracion,
            decimal totalPagado,
            CancellationToken cancellationToken = default)
        {
            var logo = await ObtenerLogoAsync(configuracion?.LogoUrl, cancellationToken);

            var nombreEmpresa = string.IsNullOrWhiteSpace(configuracion?.NombreEmpresa)
                ? "Factura"
                : configuracion!.NombreEmpresa.Trim();
            var rucONit = configuracion?.RucONit?.Trim();
            var direccionEmpresa = configuracion?.Direccion?.Trim();
            var moneda = configuracion?.Moneda?.Trim() ?? string.Empty;

            var cliente = orden.Cliente;
            var fechaEmision = DateTime.UtcNow;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

                    page.Header().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            if (logo is not null)
                                row.ConstantItem(70).Height(70).Image(logo).FitArea();

                            row.RelativeItem().PaddingLeft(logo is not null ? 12 : 0).Column(empresa =>
                            {
                                empresa.Item().Text(nombreEmpresa).FontSize(16).SemiBold();

                                // RUC/NIT y dirección son opcionales en ConfiguracionSistema:
                                // si faltan, simplemente no se imprime la línea.
                                if (!string.IsNullOrWhiteSpace(rucONit))
                                    empresa.Item().Text($"RUC/NIT: {rucONit}").FontSize(9);

                                if (!string.IsNullOrWhiteSpace(direccionEmpresa))
                                    empresa.Item().Text(direccionEmpresa).FontSize(9);
                            });

                            row.ConstantItem(150).Column(datos =>
                            {
                                datos.Item().AlignRight().Text("FACTURA").FontSize(16).SemiBold();
                                datos.Item().AlignRight().Text($"Orden #{orden.Id}").FontSize(9);
                                datos.Item().AlignRight()
                                    .Text($"Fecha: {fechaEmision.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}")
                                    .FontSize(9);
                            });
                        });

                        header.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingVertical(15).Column(content =>
                    {
                        content.Item().Text("Cliente").SemiBold();
                        content.Item().PaddingTop(3).Text(cliente.Nombre);
                        content.Item().Text($"Cédula: {cliente.Cedula}");
                        content.Item().Text($"Dirección: {cliente.Direccion}");
                        content.Item().Text($"Teléfono: {cliente.Telefono}");

                        content.Item().PaddingTop(18).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(encabezado =>
                            {
                                encabezado.Cell().Element(CeldaEncabezado).Text("Producto");
                                encabezado.Cell().Element(CeldaEncabezado).AlignRight().Text("Cant.");
                                encabezado.Cell().Element(CeldaEncabezado).AlignRight().Text("Precio unit.");
                                encabezado.Cell().Element(CeldaEncabezado).AlignRight().Text("Subtotal");

                                static IContainer CeldaEncabezado(IContainer celda) =>
                                    celda.DefaultTextStyle(t => t.SemiBold())
                                         .PaddingVertical(5)
                                         .BorderBottom(1)
                                         .BorderColor(Colors.Grey.Lighten1);
                            });

                            foreach (var detalle in detalles)
                            {
                                var nombreProducto = detalle.Producto?.Nombre ?? $"Producto #{detalle.ProductId}";

                                table.Cell().Element(Celda).Text(nombreProducto);
                                table.Cell().Element(Celda).AlignRight().Text(detalle.Cantidad.ToString(CultureInfo.InvariantCulture));
                                table.Cell().Element(Celda).AlignRight().Text(FormatearMonto(detalle.PrecioUnitario, moneda));
                                table.Cell().Element(Celda).AlignRight().Text(FormatearMonto(detalle.Subtotal, moneda));
                            }

                            static IContainer Celda(IContainer celda) =>
                                celda.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                        });

                        content.Item().PaddingTop(12).AlignRight()
                            .Text($"Total pagado: {FormatearMonto(totalPagado, moneda)}")
                            .FontSize(12).SemiBold();
                    });

                    page.Footer().Column(footer =>
                    {
                        footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        footer.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem()
                                .Text($"Orden #{orden.Id} · {fechaEmision.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)} UTC")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);

                            row.RelativeItem().AlignRight().Text(texto =>
                            {
                                texto.CurrentPageNumber().FontSize(8);
                                texto.Span(" / ").FontSize(8);
                                texto.TotalPages().FontSize(8);
                            });
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static string FormatearMonto(decimal monto, string moneda)
        {
            var valor = monto.ToString("N2", CultureInfo.InvariantCulture);
            return string.IsNullOrWhiteSpace(moneda) ? valor : $"{moneda} {valor}";
        }

        // El logo se guarda como url de blob (ConfiguracionSistema.LogoUrl, asignada por
        // ConfiguracionSistemaService.SubirLogoAsync). Se descarga con el mismo
        // BlobServiceClient — no con HttpClient — para que funcione aunque el contenedor
        // de imágenes sea privado. Si falla, la factura se emite sin logo.
        private async Task<byte[]?> ObtenerLogoAsync(string? logoUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
                return null;

            if (!Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri))
                return null;

            try
            {
                var partes = new BlobUriBuilder(uri);

                if (string.IsNullOrWhiteSpace(partes.BlobContainerName) || string.IsNullOrWhiteSpace(partes.BlobName))
                    return null;

                var blob = _blobServiceClient
                    .GetBlobContainerClient(partes.BlobContainerName)
                    .GetBlobClient(partes.BlobName);

                var existe = await blob.ExistsAsync(cancellationToken);
                if (!existe.Value)
                    return null;

                using var memoria = new MemoryStream();
                await blob.DownloadToAsync(memoria, cancellationToken);

                return memoria.ToArray();
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }
    }
}
