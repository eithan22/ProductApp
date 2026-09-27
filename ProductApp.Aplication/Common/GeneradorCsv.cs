using System.Globalization;
using System.Text;

namespace ProductApp.Aplication.Common
{
    // Arma archivos CSV en memoria con el formato que espera Excel en español (RNF-2
    // del SRS): separador ';' y UTF-8 con BOM. No se persiste nada, el archivo se
    // devuelve en la misma respuesta HTTP.
    public static class GeneradorCsv
    {
        private const char Separador = ';';

        // CRLF explícito en vez de AppendLine: Environment.NewLine cambia según el SO
        // donde corra la API y el estándar de CSV pide CRLF.
        private const string FinDeLinea = "\r\n";

        public static byte[] Generar(IEnumerable<string> encabezados, IEnumerable<IEnumerable<string>> filas)
        {
            var contenido = new StringBuilder();

            contenido.Append(string.Join(Separador, encabezados.Select(Escapar))).Append(FinDeLinea);

            foreach (var fila in filas)
                contenido.Append(string.Join(Separador, fila.Select(Escapar))).Append(FinDeLinea);

            // Encoding.GetBytes() nunca antepone el preámbulo aunque
            // encoderShouldEmitUTF8Identifier sea true — solo convierte el texto. Hay que
            // escribir el BOM a mano; si no, Excel abre el archivo como ANSI y las tildes
            // y la 'ñ' salen corruptas.
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return encoding.GetPreamble().Concat(encoding.GetBytes(contenido.ToString())).ToArray();
        }

        // Cultura invariante a propósito: el punto decimal es el que usa Excel en es-DO
        // y así el número no depende de la cultura del servidor donde corra la API.
        public static string Numero(decimal valor) => valor.ToString("0.00", CultureInfo.InvariantCulture);

        public static string Entero(int valor) => valor.ToString(CultureInfo.InvariantCulture);

        public static string Fecha(DateTime valor) => valor.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        public static string Booleano(bool valor) => valor ? "Sí" : "No";

        private static readonly char[] CaracteresFormula = { '=', '+', '-', '@', '\t', '\r' };

        private static string Escapar(string? valor)
        {
            var texto = valor ?? string.Empty;

            // Mitigación de CSV Injection (OWASP): si el valor empieza con un carácter que
            // Excel interpreta como inicio de fórmula, se neutraliza con un apóstrofo inicial.
            if (texto.Length > 0 && CaracteresFormula.Contains(texto[0]))
                texto = "'" + texto;

            // Un nombre de producto puede traer ';' o comillas. Si no se encierra entre
            // comillas, ese valor parte la fila en dos columnas al abrir el archivo.
            if (texto.Contains(Separador) || texto.Contains('"') || texto.Contains('\n') || texto.Contains('\r'))
                return $"\"{texto.Replace("\"", "\"\"")}\"";

            return texto;
        }
    }
}
