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

        private OperationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static OperationResult Success(string message = "")
        {
            return new OperationResult(true, message);
        }

        public static OperationResult Failure(string message)
        {
            return new OperationResult(false, message);
        }
    }
}