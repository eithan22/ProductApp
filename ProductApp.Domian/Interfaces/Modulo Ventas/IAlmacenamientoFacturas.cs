namespace ProductApp.Domian.Interfaces
{
    // Interfaz hermana de IAlmacenamientoImagenes, para el archivo de facturas en PDF. Vive
    // aparte a propósito: las facturas van a un contenedor privado distinto del de imágenes
    // de producto, y se descargan por la API en vez de exponerse por url pública.
    public interface IAlmacenamientoFacturas
    {
        Task<string> SubirAsync(byte[] contenido, string nombreBlob, CancellationToken cancellationToken = default);

        Task<byte[]?> DescargarAsync(string nombreBlob, CancellationToken cancellationToken = default);

        // Existe para poder cumplir la promesa de la Política de Privacidad: el cliente puede
        // pedir que se borre la factura de una orden. Es idempotente: borrar una factura que ya
        // no está no es un error.
        Task EliminarAsync(string nombreBlob, CancellationToken cancellationToken = default);
    }
}
