namespace ProductApp.Tests.Integration
{
    // Cabecera real de cada formato + relleno. La validación solo mira los primeros bytes,
    // así que no hace falta una imagen completa ni meter archivos binarios en el repo.
    internal static class ImagenesDePrueba
    {
        public static byte[] Png() => ConRelleno(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        public static byte[] Jpeg() => ConRelleno(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });

        // "RIFF" + 4 bytes de tamaño + "WEBP"
        public static byte[] Webp() => ConRelleno(new byte[]
            { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 });

        // Cabecera "MZ": un ejecutable de Windows renombrado a .jpg, el caso del hallazgo.
        public static byte[] NoEsImagen() => ConRelleno(new byte[] { 0x4D, 0x5A, 0x90, 0x00 });

        private static byte[] ConRelleno(byte[] firma, int total = 64)
        {
            var bytes = new byte[total];
            firma.CopyTo(bytes, 0);
            return bytes;
        }
    }
}
