namespace Web.Models.Modelo_Busqueda.BusquedaModels
{
    public class BusquedaGlobalModel
    {
        // El texto llega normalizado desde la API y es el que se resalta en cada fila.
        public string Texto { get; set; } = string.Empty;

        public int TotalCoincidencias { get; set; }

        public BusquedaGrupoModel<BusquedaProductoItemModel> Productos { get; set; } = new();
        public BusquedaGrupoModel<BusquedaClienteItemModel> Clientes { get; set; } = new();
        public BusquedaGrupoModel<BusquedaOrdenItemModel> Ordenes { get; set; } = new();
        public BusquedaGrupoModel<BusquedaProveedorItemModel> Proveedores { get; set; } = new();
    }
}
