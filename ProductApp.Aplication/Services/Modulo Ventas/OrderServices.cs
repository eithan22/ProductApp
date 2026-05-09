using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class OrderServices : IOrdenServices
    {
        private readonly IOrdenRepository _ordenRepository;

        public OrderServices(IOrdenRepository ordenRepository)
        {
            _ordenRepository = ordenRepository;
        }

        public Task<OperationResultD<OrdenResponseDto>> CreateAsync(CreateOrdenDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResultD<bool>> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResultD<bool>> DisableAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResultD<List<OrdenResponseDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResultD<OrdenResponseDto>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResultD<OrdenResponseDto>> UpdateAsync(UpdateOrdenDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
