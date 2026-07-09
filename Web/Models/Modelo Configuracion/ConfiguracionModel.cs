namespace Web.Models.Modelo_Configuracion
{
    public class ConfiguracionModel
    {
        public int CantidadMinimaInventarioDefecto { get; set; }
        public int DuracionTokenMinutos { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
    }
}
