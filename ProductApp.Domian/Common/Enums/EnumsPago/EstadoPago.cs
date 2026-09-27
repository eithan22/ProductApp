using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Common.Enums.EnumsPago
{
    // Ojo con el nombre (deuda documentada en M14, auditoría de seguridad): "Pendiente" no
    // significa "dinero sin confirmar" — el pago ya se registró y el dinero ya entró a caja.
    // Significa "este pago todavía no saldó el total de la orden" (ver Orden.CalcularSaldoPendiente).
    // Un pago parcial queda en Pendiente para siempre; solo el pago que completa el saldo
    // pasa a Completado. Renombrar esto bien (ej. a "SaldaLaOrden"/"NoSaldaLaOrden" o eliminar
    // el campo y derivarlo del saldo) exige migrar la columna, que se persiste como texto
    // (ver PagoConfig.HasConversion<string>()) — no se hizo en M14 porque no cambia ningún
    // comportamiento observable (la única pantalla que mostraba el enum crudo ya se corrigió
    // en Web/Views/Orden/Details.cshtml).
    //
    // Fallido es código muerto: nada en el sistema lo asigna. No hay pasarela de pago —se
    // registra dinero ya recibido en caja—, así que un pago no puede "fallar" en este modelo.
    // Se conserva para cuando exista una integración con pasarela real.
    public enum EstadoPago
    {
        Pendiente = 0,
        Completado = 1,
        Fallido = 2
    }
}
