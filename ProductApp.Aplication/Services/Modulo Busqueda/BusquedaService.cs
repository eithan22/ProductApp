using ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Busqueda;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class BusquedaService : IBusquedaServices
    {
        // Tope por categoría (docs/07-sdd.md §4.2): evita que un texto muy genérico
        // devuelva el catálogo completo dentro de un dropdown.
        private const int LimitePorCategoria = 10;

        private readonly IProductoRepository _productoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IOrdenRepository _ordenRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IMapperBusqueda _mapperBusqueda;

        public BusquedaService(
            IProductoRepository productoRepository,
            IClienteRepository clienteRepository,
            IOrdenRepository ordenRepository,
            IProveedorRepository proveedorRepository,
            IMapperBusqueda mapperBusqueda)
        {
            _productoRepository = productoRepository;
            _clienteRepository = clienteRepository;
            _ordenRepository = ordenRepository;
            _proveedorRepository = proveedorRepository;
            _mapperBusqueda = mapperBusqueda;
        }

        public async Task<OperationResultD<BusquedaGlobalResponseDto>> BuscarGlobalAsync(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return OperationResultD<BusquedaGlobalResponseDto>.Failure("Debe proporcionar un texto de búsqueda");

            var textoNormalizado = texto.Trim();

            // Las cuatro consultas van en secuencia, no con Task.WhenAll: los cuatro
            // repositorios comparten la misma instancia scoped de AppDbContext de la
            // request y EF Core no admite dos operaciones simultáneas sobre ella.
            var productos = await _productoRepository.BuscarProductosAsync(textoNormalizado, null);
            var clientes = await _clienteRepository.BuscarClientesAsync(textoNormalizado, null, null);
            var ordenes = await _ordenRepository.BuscarOrdenesAsync(textoNormalizado);
            var proveedores = await _proveedorRepository.BuscarProveedoresAsync(textoNormalizado);

            var response = new BusquedaGlobalResponseDto
            {
                Texto = textoNormalizado,
                Productos = ArmarGrupo(productos, _mapperBusqueda.MapToProductoItem),
                Clientes = ArmarGrupo(clientes, _mapperBusqueda.MapToClienteItem),
                Ordenes = ArmarGrupo(ordenes, o => _mapperBusqueda.MapToOrdenItem(o.Orden, o.CantidadProductos, o.TotalPagado)),
                Proveedores = ArmarGrupo(proveedores, _mapperBusqueda.MapToProveedorItem)
            };

            response.TotalCoincidencias =
                response.Productos.TotalEncontrados +
                response.Clientes.TotalEncontrados +
                response.Ordenes.TotalEncontrados +
                response.Proveedores.TotalEncontrados;

            return OperationResultD<BusquedaGlobalResponseDto>.Success(response, "Búsqueda realizada exitosamente");
        }

        // El recorte al límite y el aviso de "hay más" se resuelven en un solo lugar para
        // las cuatro categorías: así ninguna puede quedar con un comportamiento distinto.
        private static BusquedaGrupoDto<TDto> ArmarGrupo<TOrigen, TDto>(
            List<TOrigen> encontrados,
            Func<TOrigen, TDto> mapear)
        {
            return new BusquedaGrupoDto<TDto>
            {
                TotalEncontrados = encontrados.Count,
                HayMasResultados = encontrados.Count > LimitePorCategoria,
                Items = encontrados.Take(LimitePorCategoria).Select(mapear).ToList()
            };
        }
    }
}
