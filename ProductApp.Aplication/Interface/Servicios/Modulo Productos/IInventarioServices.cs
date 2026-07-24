using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IInventarioServices
    {
        Task<OperationResultD<InventarioResponseDto>> AgregarStockAsync(MovimientoStockDto agregarStockDto);

        Task<OperationResultD<InventarioResponseDto>> DescontarStockAsync(MovimientoStockDto descontarStockDto);

        // El método AjustarStockAsync se utiliza para ajustar el stock de un producto a una cantidad específica, independientemente de la cantidad actual. Esto es útil para corregir errores de inventario o para sincronizar el stock con una cantidad real después de una auditoría.
        Task<OperationResultD<InventarioResponseDto>> AjustarStockAsync(AjustarStockDto ajustarStockDto, int usuarioSolicitanteId);

        // Este método se puede usar para obtener el inventario de un producto específico, incluyendo la cantidad actual, la cantidad mínima y cualquier otra información relevante. Es útil para mostrar el estado del inventario en la interfaz de usuario o para tomar decisiones basadas en el stock disponible.
        Task<OperationResultD<InventarioResponseDto>> ObtenerInventarioAsync(int productoId);

        Task<OperationResultD<List<InventarioResponseDto>>> ObtenerStockBajoAsync();

        // Este método se puede usar para obtener una lista de todos los inventarios, lo que es útil para mostrar un resumen del estado del inventario en la interfaz de usuario o para realizar análisis y reportes sobre el inventario.
        Task<OperationResultD<PagedResult<InventarioResponseDto>>> ObtenerTodosInventariosAsync(int pageNumber = 1, int pageSize = 10);
    }
}
