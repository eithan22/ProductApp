namespace Web.Models.Modelo_Busqueda.BusquedaModels
{
    // Espejo de BusquedaGrupoDto<T>: las cuatro categorías vienen con la misma forma
    // (items + cuántos hay en total + si la API recortó la lista).
    public class BusquedaGrupoModel<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalEncontrados { get; set; }
        public bool HayMasResultados { get; set; }
    }
}
