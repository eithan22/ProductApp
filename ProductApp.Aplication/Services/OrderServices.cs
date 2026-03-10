using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface;
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


        public Task<OrdenResponseDto> CreateAsync(CreateOrdenDto dto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task DisableAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrdenResponseDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OrdenResponseDto> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OrdenResponseDto> UpdateAsync(UpdateOrdenDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
