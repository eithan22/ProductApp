using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Common.Enums.EnumsProveedor;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Productos
{
    public class ValidatorBusinessProducto : IValidatorBusinessProducto
    {
        private readonly IProductoRepository _productoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IProveedorRepository _proveedorRepository;

        public ValidatorBusinessProducto(
            IProductoRepository productoRepository,
            ICategoriaRepository categoriaRepository,
            IProveedorRepository proveedorRepository)
        {
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;
            _proveedorRepository = proveedorRepository;
        }

        public async Task<OperationResult> ValidarCreateProductoAsync(CreateProductoDto dto)
        {
            if (await _productoRepository.ExisteAsync(p => p.Nombre == dto.Nombre))
                return OperationResult.Failure("Ya existe un producto con ese nombre.");

            return await ValidarReferenciasAsync(dto.CategoriaId, dto.ProveedorId);
        }

        public async Task<OperationResult> ValidarUpdateProductoAsync(UpdateProductoDto dto, Producto producto)
        {
            if (await _productoRepository.ExisteAsync(p => p.Nombre == dto.Nombre && p.Id != dto.Id))
                return OperationResult.Failure("Ya existe otro producto con ese nombre.");

            return await ValidarReferenciasAsync(dto.CategoriaId, dto.ProveedorId);
        }

        public async Task<OperationResult> ValidarDisableProductoAsync(Producto producto)
        {
            if (producto.Estado == EstadoProducto.Inactivo)
                return OperationResult.Failure("El producto ya está inactivo.");

            return OperationResult.Success();
        }

        // Categoría y proveedor se comprueban contra la base ANTES de que el mapper toque la
        // entidad: un id inexistente reventaría recién en SaveChanges como error de FK (500)
        // en vez de salir como Failure con un mensaje entendible. GetByIdAsync ya filtra
        // EstaEliminado, así que una categoría desactivada tampoco pasa.
        private async Task<OperationResult> ValidarReferenciasAsync(int categoriaId, int? proveedorId)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(categoriaId);

            if (categoria == null)
                return OperationResult.Failure("La categoría seleccionada no existe.");

            // ProveedorId null es válido: significa "producto sin proveedor asignado" (RF-3.7.2).
            if (proveedorId.HasValue)
            {
                var proveedor = await _proveedorRepository.GetByIdAsync(proveedorId.Value);

                if (proveedor == null)
                    return OperationResult.Failure("El proveedor seleccionado no existe.");

                if (proveedor.Estado == EstadoProveedor.Inactivo)
                    return OperationResult.Failure("El proveedor seleccionado no está activo.");
            }

            return OperationResult.Success();
        }
    }
}
