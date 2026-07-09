using System.Globalization;

namespace Web.Helpers
{
    public static class MonedaHelper
    {
        private static readonly Dictionary<string, CultureInfo> Culturas = new()
        {
            ["USD"] = new CultureInfo("en-US"),
            ["DOP"] = new CultureInfo("es-DO"),
            ["EUR"] = new CultureInfo("es-ES"),
            ["MXN"] = new CultureInfo("es-MX"),
        };

        public static string Formatear(decimal monto, string? moneda)
        {
            var culture = !string.IsNullOrWhiteSpace(moneda) && Culturas.TryGetValue(moneda, out var c)
                ? c
                : CultureInfo.CurrentCulture;

            return monto.ToString("C", culture);
        }
    }
}
