using ProductApp.Aplication.Dtos.ReporteDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Reportes;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class ReporteService : IReporteServices
    {
        private readonly IReporteRepository _reporteRepository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IMapperReporte _mapperReporte;

        public ReporteService(IReporteRepository reporteRepository, IInventarioRepository inventarioRepository, IMapperReporte mapperReporte)
        {
            _reporteRepository = reporteRepository;
            _inventarioRepository = inventarioRepository;
            _mapperReporte = mapperReporte;
        }

        public async Task<OperationResultD<List<VentaPorFechaDto>>> ObtenerVentasPorFechaAsync(DateTime? desde, DateTime? hasta)
        {
            var (desdeResuelto, hastaResuelto) = ResolverRango(desde, hasta);
            if (desdeResuelto > hastaResuelto)
                return OperationResultD<List<VentaPorFechaDto>>.Failure("La fecha 'desde' no puede ser mayor a la fecha 'hasta'");

            var data = await _reporteRepository.ObtenerVentasPorFechaAsync(desdeResuelto, hastaResuelto);
            var response = data.Select(_mapperReporte.MapToVentaPorFechaDto).ToList();

            return OperationResultD<List<VentaPorFechaDto>>.Success(response, "Ventas por fecha obtenidas exitosamente");
        }

        public async Task<OperationResultD<List<VentaPorProductoDto>>> ObtenerVentasPorProductoAsync(DateTime? desde, DateTime? hasta)
        {
            var (desdeResuelto, hastaResuelto) = ResolverRango(desde, hasta);
            if (desdeResuelto > hastaResuelto)
                return OperationResultD<List<VentaPorProductoDto>>.Failure("La fecha 'desde' no puede ser mayor a la fecha 'hasta'");

            var data = await _reporteRepository.ObtenerVentasPorProductoAsync(desdeResuelto, hastaResuelto);
            var response = data.Select(_mapperReporte.MapToVentaPorProductoDto).ToList();

            return OperationResultD<List<VentaPorProductoDto>>.Success(response, "Ventas por producto obtenidas exitosamente");
        }

        public async Task<OperationResultD<List<VentaPorVendedorDto>>> ObtenerVentasPorVendedorAsync(DateTime? desde, DateTime? hasta, int? usuarioId, int usuarioAutenticadoId, bool esAdministrador)
        {
            var (desdeResuelto, hastaResuelto) = ResolverRango(desde, hasta);
            if (desdeResuelto > hastaResuelto)
                return OperationResultD<List<VentaPorVendedorDto>>.Failure("La fecha 'desde' no puede ser mayor a la fecha 'hasta'");

            var filtroUsuarioId = esAdministrador ? usuarioId : usuarioAutenticadoId;

            var data = await _reporteRepository.ObtenerVentasPorVendedorAsync(desdeResuelto, hastaResuelto, filtroUsuarioId);
            var response = data.Select(_mapperReporte.MapToVentaPorVendedorDto).ToList();

            return OperationResultD<List<VentaPorVendedorDto>>.Success(response, "Ventas por vendedor obtenidas exitosamente");
        }

        public async Task<OperationResultD<List<InventarioActualDto>>> ObtenerInventarioActualAsync()
        {
            var (inventarios, _) = await _inventarioRepository.GetAllConProductoAsync(pageNumber: 1, pageSize: int.MaxValue);
            var response = inventarios.Select(_mapperReporte.MapToInventarioActualDto).ToList();

            return OperationResultD<List<InventarioActualDto>>.Success(response, "Inventario actual obtenido exitosamente");
        }

        public async Task<OperationResultD<List<ProductoMasVendidoDto>>> ObtenerProductosMasVendidosAsync(DateTime? desde, DateTime? hasta, int top)
        {
            if (top <= 0)
                return OperationResultD<List<ProductoMasVendidoDto>>.Failure("El parámetro 'top' debe ser mayor a cero");

            var (desdeResuelto, hastaResuelto) = ResolverRango(desde, hasta);
            if (desdeResuelto > hastaResuelto)
                return OperationResultD<List<ProductoMasVendidoDto>>.Failure("La fecha 'desde' no puede ser mayor a la fecha 'hasta'");

            var data = await _reporteRepository.ObtenerProductosMasVendidosAsync(desdeResuelto, hastaResuelto, top);
            var response = data.Select(_mapperReporte.MapToProductoMasVendidoDto).ToList();

            return OperationResultD<List<ProductoMasVendidoDto>>.Success(response, "Productos más vendidos obtenidos exitosamente");
        }

        public async Task<OperationResultD<IngresosTotalesDto>> ObtenerIngresosTotalesAsync(DateTime? desde, DateTime? hasta)
        {
            var (desdeResuelto, hastaResuelto) = ResolverRango(desde, hasta);
            if (desdeResuelto > hastaResuelto)
                return OperationResultD<IngresosTotalesDto>.Failure("La fecha 'desde' no puede ser mayor a la fecha 'hasta'");

            var (total, cantidadPagos) = await _reporteRepository.ObtenerIngresosTotalesAsync(desdeResuelto, hastaResuelto);
            var ticketPromedio = cantidadPagos > 0 ? total / cantidadPagos : 0;

            var response = _mapperReporte.MapToIngresosTotalesDto(desdeResuelto, hastaResuelto, total, cantidadPagos, ticketPromedio);

            return OperationResultD<IngresosTotalesDto>.Success(response, "Ingresos totales obtenidos exitosamente");
        }

        private static (DateTime Desde, DateTime Hasta) ResolverRango(DateTime? desde, DateTime? hasta)
        {
            var hastaResuelto = hasta ?? DateTime.UtcNow;
            var desdeResuelto = desde ?? hastaResuelto.AddDays(-30);
            return (desdeResuelto, hastaResuelto);
        }
    }
}
