namespace ProductApp.Aplication.Common
{
    // Formato reconocido por la firma binaria del archivo, no por lo que declaró el cliente:
    // la extensión del nombre y el Content-Type del request los elige quien sube el archivo.
    public sealed record FormatoImagen(string ContentType, string Extension);

    // Lee solo la cabecera del stream y lo deja rebobinado en 0 para que la subida real
    // al storage lo pueda leer completo.
    public static class DetectorFormatoImagen
    {
        // 12 bytes: es lo que necesita WEBP ("RIFF" + tamaño + "WEBP"). JPEG y PNG piden menos.
        private const int BytesCabecera = 12;

        private static readonly byte[] FirmaJpeg = { 0xFF, 0xD8, 0xFF };
        private static readonly byte[] FirmaPng = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] FirmaRiff = { 0x52, 0x49, 0x46, 0x46 }; // "RIFF"
        private static readonly byte[] FirmaWebp = { 0x57, 0x45, 0x42, 0x50 }; // "WEBP"

        // null = el contenido no es JPEG, PNG ni WEBP, o no se pudo verificar.
        public static async Task<FormatoImagen?> DetectarAsync(
            Stream contenido, CancellationToken cancellationToken = default)
        {
            // Sin poder rebobinar no se puede verificar y subir el mismo stream; se rechaza
            // antes que subir algo a ciegas. Los streams de IFormFile sí son seekable.
            if (contenido is null || !contenido.CanRead || !contenido.CanSeek)
                return null;

            contenido.Position = 0;

            var cabecera = new byte[BytesCabecera];
            var leidos = await contenido.ReadAtLeastAsync(
                cabecera, BytesCabecera, throwOnEndOfStream: false, cancellationToken);

            contenido.Position = 0;

            return Detectar(cabecera.AsSpan(0, leidos));
        }

        private static FormatoImagen? Detectar(ReadOnlySpan<byte> cabecera)
        {
            if (cabecera.StartsWith(FirmaJpeg))
                return new FormatoImagen("image/jpeg", ".jpg");

            if (cabecera.StartsWith(FirmaPng))
                return new FormatoImagen("image/png", ".png");

            if (cabecera.Length >= 12 && cabecera.StartsWith(FirmaRiff) && cabecera.Slice(8, 4).SequenceEqual(FirmaWebp))
                return new FormatoImagen("image/webp", ".webp");

            return null;
        }
    }
}
