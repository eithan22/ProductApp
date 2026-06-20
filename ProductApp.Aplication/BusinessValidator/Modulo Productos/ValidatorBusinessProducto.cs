using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Productos
{
    public class ValidatorBusinessProducto : IValidatorBusinessProducto
    {
        private readonly IProductoRepository _productoRepository;

        public ValidatorBusinessProducto(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<OperationResult> ValidarCreateProductoAsync(CreateProductoDto dto)
        {
            if (await _productoRepository.ExisteAsync(p => p.Nombre == dto.Nombre))
                return OperationResult.Failure("Ya existe un producto con ese nombre.");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarUpdateProductoAsync(UpdateProductoDto dto, Producto producto)
        {
            if (await _productoRepository.ExisteAsync(p => p.Nombre == dto.Nombre && p.Id != dto.Id))
                return OperationResult.Failure("Ya existe otro producto con ese nombre.");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarDisableProductoAsync(Producto producto)
        {
            if (producto.Estado == EstadoProducto.Inactivo)
                return OperationResult.Failure("El producto ya está inactivo.");

            return OperationResult.Success();
        }
    }
}
