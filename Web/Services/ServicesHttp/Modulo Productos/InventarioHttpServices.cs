using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using Web.Models.Modelo_Productos.InventarioModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.Mappers.Modulo_Productos;

namespace Web.Services.ServicesHttp.Modulo_Productos
{
    public class InventarioHttpServices : IInventarioHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IInventarioEndpoint _inventarioEndpoint;

        public InventarioHttpServices(IBaseHttpServices baseHttpServices, IInventarioEndpoint inventarioEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _inventarioEndpoint = inventarioEndpoint;
        }

        public async Task<PagedResult<InventarioModel>> GetAllInventariosAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _baseHttpServices.GetAsync<PagedResult<InventarioModel>>(
                $"{_inventarioEndpoint.GetAll}?pageNumber={pageNumber}&pageSize={pageSize}");
        }

        public async Task<List<InventarioModel>> GetStockBajoAsync()
        {
            return await _baseHttpServices.GetAsync<List<InventarioModel>>(_inventarioEndpoint.GetStockBajo);
        }

        public async Task<InventarioModel> GetInventarioPorProductoAsync(int productoId)
        {
            return await _baseHttpServices.GetAsync<InventarioModel>($"{_inventarioEndpoint.GetInventarioPorProducto}{productoId}");
        }

        public async Task<InventarioModel> AgregarStockAsync(MovimientoStockModel model)
        {
            var dto = InventarioMapperM.MapMovimientoStockDto(model);
            return await _baseHttpServices.PostAsync<MovimientoStockDto, InventarioModel>(_inventarioEndpoint.AgregarStock, dto);
        }

        public async Task<InventarioModel> DescontarStockAsync(MovimientoStockModel model)
        {
            var dto = InventarioMapperM.MapMovimientoStockDto(model);
            return await _baseHttpServices.PostAsync<MovimientoStockDto, InventarioModel>($"{_inventarioEndpoint.DescontarStock}{model.ProductoId}", dto);
        }

        public async Task<InventarioModel> AjustarInventarioAsync(AjustarStockModel model)
        {
            var dto = InventarioMapperM.MapAjustarStockDto(model);
            return await _baseHttpServices.PutAsync<AjustarStockDto, InventarioModel>($"{_inventarioEndpoint.AjustarInventario}?productoId={model.ProductoId}", dto);
        }
    }
}
