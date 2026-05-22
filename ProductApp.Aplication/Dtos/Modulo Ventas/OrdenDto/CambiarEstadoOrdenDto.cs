using ProductApp.Domian.Common.Enums.EnumsOrden;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto
{
    public class CambiarEstadoOrdenDto
    {
        public int Id { get; set; }
        public EstadoOrden NuevoEstado { get; set; } = EstadoOrden.Pendiente;
    }
}
