using Web.Services.Interfaces.IEndPoints.Modulo_Productos;

namespace Web.Services.EndPoints.Modulo_Productos
{
    public class InventarioEndpoint : IInventarioEndpoint
    {
        public string GetAll => "Inventario/GetAllInventarios";
        public string GetInventarioPorProducto => "Inventario/GetInventarioPorProducto/";
        public string GetStockBajo => "Inventario/GetStockBajo";
        public string AgregarStock => "Inventario/AgregarStock";
        public string DescontarStock => "Inventario/DescontarStock/";
        public string AjustarInventario => "Inventario/AjustarInventario";
    }
}
