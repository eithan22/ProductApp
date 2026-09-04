using Web.Models.Modelo_Productos.InventarioModels;
using Web.Models.Modelo_Reportes.ReporteModels;
using Web.Models.Modelo_Ventas.OrdenModels;

namespace Web.Models
{
    public class HomeDashboardModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool EsAdministrador { get; set; }

        // KPIs. null = no disponible (sin permiso o falló la llamada).
        // Se muestra "—", nunca un 0 que sería mentira.
        public decimal? IngresosHoy { get; set; }
        public int? OrdenesPendientes { get; set; }
        public int? ProductosStockBajo { get; set; }
        public int? ClientesActivos { get; set; }

        public List<VentaPorFechaModel> VentasSemana { get; set; } = new();
        public List<OrdenModel> OrdenesRecientes { get; set; } = new();
        public List<InventarioModel> AlertasStock { get; set; } = new();

        // true si algún bloque no se pudo cargar; la vista lo avisa.
        public bool HuboErrores { get; set; }
    }
}
