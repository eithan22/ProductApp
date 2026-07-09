namespace Web.Models.Modelo_Productos.InventarioModels
{
    public class AjustarStockModel
    {
        public int ProductoId { get; set; }
        public int NuevoStock { get; set; }
        public int? NuevoStockMinimo { get; set; }
    }
}
