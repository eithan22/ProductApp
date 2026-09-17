namespace ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto
{
    public class BusquedaGlobalResponseDto
    {
        // Se devuelve el texto ya normalizado (sin espacios sobrantes) porque la Web lo usa
        // para resaltar la coincidencia dentro de cada resultado.
        public string Texto { get; set; } = string.Empty;

        // Suma de los TotalEncontrados de las cuatro categorías: es el "N coincidencias"
        // del encabezado del dropdown.
        public int TotalCoincidencias { get; set; }

        public BusquedaGrupoDto<BusquedaProductoItemDto> Productos { get; set; } = new();
        public BusquedaGrupoDto<BusquedaClienteItemDto> Clientes { get; set; } = new();
        public BusquedaGrupoDto<BusquedaOrdenItemDto> Ordenes { get; set; } = new();

        // Cuarta categoría incluida desde el inicio porque el módulo Proveedores ya está
        // implementado (docs/03-prd.md §3.8, criterio de aceptación).
        public BusquedaGrupoDto<BusquedaProveedorItemDto> Proveedores { get; set; } = new();
    }
}
