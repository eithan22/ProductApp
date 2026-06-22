namespace ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto
{
    public class CambiarEstadoOrdenDto
    {
        public int Id { get; set; }
        public string NuevoEstado { get; set; } = string.Empty;
    }
}
