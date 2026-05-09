using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Result.ApiResponses
{
    public class ApiResponseT<T>
    {
        public bool Success { get;  set; }
        public string Message { get;  set; } = string.Empty;

        public T? Data { get;  set; }

        public ApiResponseT()
        {
        }

        private ApiResponseT(bool success, string message, T? data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static ApiResponseT<T> SuccessResponse(T data, string message = "Operación exitosa")
        {
            return new ApiResponseT<T>(true, message, data);
        }

        public static ApiResponseT<T> FailureResponse(string message)
        {
            return new ApiResponseT<T>(false, message, default);
        }
    }
}
