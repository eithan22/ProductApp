namespace ProductApp.Aplication.Dtos.Modulo_Configuracion
{
    public class ActualizarConfiguracionSistemaDto
    {
        public int CantidadMinimaInventarioDefecto { get; set; }
        public int DuracionTokenMinutos { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
    }
}
