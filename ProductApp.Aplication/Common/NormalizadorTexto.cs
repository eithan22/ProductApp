using System.Globalization;
using System.Text;

namespace ProductApp.Aplication.Common
{
    // Normaliza nombres para comparar sin castigar al usuario por tildes, mayúsculas o
    // espacios de más: "  ELECTRONICA " y "Electrónica" dan la misma clave.
    // Se usa solo para BUSCAR coincidencias, nunca para guardar: lo que se persiste es
    // siempre el nombre real de la entidad que ya está en la base.
    public static class NormalizadorTexto
    {
        public static string Normalizar(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            // Espacios internos repetidos colapsados a uno: "Grupo  Eléctrico" == "Grupo Eléctrico".
            var compacto = string.Join(' ', valor.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

            // FormD separa la letra de su tilde; al descartar las marcas diacríticas
            // queda la letra base ("é" → "e", "ñ" → "n").
            var descompuesto = compacto.Normalize(NormalizationForm.FormD);
            var limpio = new StringBuilder(descompuesto.Length);

            foreach (var caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                    limpio.Append(caracter);
            }

            return limpio.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }
    }
}
