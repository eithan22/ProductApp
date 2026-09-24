using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Result.OperationResult
{
    public class OperationResult
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;

        public string Message { get; private set; } = string.Empty;

        // Motivo real del fallo, solo para los logs del servidor. Cuando la respuesta al cliente
        // tiene que ser genérica (por ejemplo en el login, para no revelar si un usuario existe),
        // Message lleva el texto genérico y esta propiedad conserva la causa exacta.
        // Si no se especifica un motivo, cae en Message: el comportamiento de siempre.
        public string MotivoInterno { get; private set; } = string.Empty;

        private OperationResult(bool isSuccess, string message, string? motivoInterno = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            MotivoInterno = string.IsNullOrWhiteSpace(motivoInterno) ? message : motivoInterno;
        }

        public static OperationResult Success(string message = "")
        {
            return new OperationResult(true, message);
        }

        public static OperationResult Failure(string message, string? motivoInterno = null)
        {
            return new OperationResult(false, message, motivoInterno);
        }
    }
}