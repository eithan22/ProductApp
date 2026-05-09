using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Result.OperationResult
{

    public class OperationResultD<T> 
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;

        public string Message { get; private set; } = string.Empty;

        public T? Data { get; private set; }

        private OperationResultD(bool isSuccess, string message, T? data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data= data;
        }

        public static OperationResultD<T> Success(T? data ,string Message = "" )
        {
            return new OperationResultD<T>(true, Message, data);
        }

        public static OperationResultD<T> Failure(string message)
        {
            return new OperationResultD<T>(false, message, default);
        }

    }




}
