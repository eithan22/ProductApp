namespace ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto
{
    // Envoltura común a las cuatro categorías de la búsqueda global. Las cuatro se pintan
    // igual en el dropdown (título del grupo + contador + filas + aviso de "hay más"), así
    // que comparten forma en vez de repetir dos propiedades sueltas por cada una.
    public class BusquedaGrupoDto<T>
    {
        public List<T> Items { get; set; } = new();

        // Cuántas coincidencias hay en total, antes de recortar al límite por categoría.
        public int TotalEncontrados { get; set; }

        // true cuando TotalEncontrados supera el límite: la Web muestra
        // "hay más resultados, refiná la búsqueda".
        public bool HayMasResultados { get; set; }
    }
}
