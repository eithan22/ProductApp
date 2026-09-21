using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class Inventario : BaseEntity
    {
        public int CantidadActual { get; private set; }
        public int CantidadMinima { get; private set; }
        public int ProductoId { get; private set; }
        public DateTime UltimaActualizacion { get; private set; }

        public Producto Producto { get; private set; } = null!;

        // Tope de unidades por producto. Una tienda no recibe ni almacena un millón de
        // unidades de un mismo artículo; el límite existe para que la suma de entradas
        // nunca pueda desbordar el int y dejar la cantidad en negativo.
        public const int CantidadMaximaStock = 1_000_000;

        protected Inventario() { }

        public Inventario(int cantidadActual, int cantidadMinima, int productoId)
        {
            if (cantidadActual < 0)
                throw new ValidacionDominioException("CantidadActual", "La cantidad actual no puede ser negativa.");

            if (cantidadMinima < 0)
                throw new ValidacionDominioException("CantidadMinima", "La cantidad mínima no puede ser negativa.");

            if (productoId <= 0)
                throw new ValidacionDominioException("ProductoId", "El id del producto no es válido.");

            CantidadActual = cantidadActual;
            CantidadMinima = cantidadMinima;
            ProductoId = productoId;
            UltimaActualizacion = DateTime.UtcNow;
        }

        // --- Comportamiento de dominio ---

        public bool EsStockBajo() => CantidadActual <= CantidadMinima;

        public void AjustarStock(int nuevoStock)
        {
            if (nuevoStock < 0)
                throw new ValidacionDominioException("Stock", "El stock no puede ser negativo.");

            if (nuevoStock > CantidadMaximaStock)
                throw new ValidacionDominioException("Stock",
                    $"El stock no puede superar las {CantidadMaximaStock} unidades.");

            CantidadActual = nuevoStock;
            UltimaActualizacion = DateTime.UtcNow;
            ActualizarFechaModificacion();
        }

        public void AjustarStockMinimo(int nuevoMinimo)
        {
            if (nuevoMinimo < 0)
                throw new ValidacionDominioException("StockMinimo", "El stock mínimo no puede ser negativo.");

            CantidadMinima = nuevoMinimo;
            UltimaActualizacion = DateTime.UtcNow;
            ActualizarFechaModificacion();
        }

        public void RegistrarEntradaStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ValidacionDominioException("Cantidad", "La cantidad de entrada debe ser mayor a cero.");

            // La resta se hace sobre el tope, nunca la suma sobre la cantidad actual:
            // así la propia comprobación es imposible de desbordar.
            if (cantidad > CantidadMaximaStock - CantidadActual)
                throw new ValidacionDominioException("Cantidad",
                    $"Stock máximo excedido. Máximo permitido: {CantidadMaximaStock}, actual: {CantidadActual}, solicitado: {cantidad}.");

            CantidadActual += cantidad;
            UltimaActualizacion = DateTime.UtcNow;
            ActualizarFechaModificacion();
        }

        public void RegistrarSalidaStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ValidacionDominioException("Cantidad", "La cantidad de salida debe ser mayor a cero.");

            if (cantidad > CantidadActual)
                throw new ValidacionDominioException("Cantidad",
                    $"Stock insuficiente. Disponible: {CantidadActual}, solicitado: {cantidad}.");

            CantidadActual -= cantidad;
            UltimaActualizacion = DateTime.UtcNow;
            ActualizarFechaModificacion();
        }
    }
}
