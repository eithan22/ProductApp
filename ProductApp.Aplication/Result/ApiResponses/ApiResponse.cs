using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Result.ApiResponses
{
    public class ApiResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static ApiResponse SuccessResponse(string message = "Operación exitosa")
        {
            return new ApiResponse(true, message);
        }

        public static ApiResponse FailureResponse(string message)
        {
            return new ApiResponse(false, message);
        }
    }
}
