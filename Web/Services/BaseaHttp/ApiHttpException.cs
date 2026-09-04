using System.Net;

namespace Web.Services.Base
{
    /// <summary>
    /// Excepción de la capa HTTP que además del mensaje transporta el status code
    /// de la respuesta de la API. Hereda de Exception para no romper ningún
    /// catch(Exception) existente en los controllers.
    /// </summary>
    public class ApiHttpException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ApiHttpException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
