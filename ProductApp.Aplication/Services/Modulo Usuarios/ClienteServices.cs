using Microsoft.IdentityModel.Tokens;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class ClienteServices : IClienteServices
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteServices(IClienteRepository clienterepositoy)
        {
            _clienteRepository = clienterepositoy;
        }

        public async Task<ClienteResponseDto> CreateAsync(CreateClienteDto dto)
        {
            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Email = dto.Correo,
                Telefono = dto.Telefono,
                Cedula = dto.Cedula,
                Direccion = dto.Direccion,
                Estado = EstadoCliente.Activo

            };
            await _clienteRepository.CreateAsync(cliente);

            var clienteResponse = new ClienteResponseDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Cedula = cliente.Cedula,
                Direccion = cliente.Direccion,
                estado = cliente.Estado
            };

            return clienteResponse;

        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El id no puede ser menor o igual a 0");
            }

            var cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null)
            {
                throw new Exception("El cliente no fue encontrado");

            }

            await _clienteRepository.DeleteAsync(id);





        }


        public async Task DisableAsync(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El id no puede ser menor o igual a 0");
            }

            var cliente = _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new Exception("El cliente no fue encontrado");
            }

            await _clienteRepository.DisebleAsync(id);
        }



        public async Task<List<ClienteResponseDto>> GetAllAsync()
        {
            var clientes = await _clienteRepository.GetAllAsync();

            var clienteresponsedto = clientes.Select(c => new ClienteResponseDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email,
                Telefono = c.Telefono,
                Cedula = c.Cedula,
                Direccion = c.Direccion,
                estado = c.Estado
            }).ToList();

            return clienteresponsedto;



        }

        public async Task<ClienteResponseDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El id no puede ser menor o igual a 0");
            }

            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new Exception("El cliente no fue encontrado");
            }

            var clienteResponsedto = new ClienteResponseDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Cedula = cliente.Cedula,
                Direccion = cliente.Direccion,
                estado = cliente.Estado
            };

            return clienteResponsedto;





        }



        public async Task<ClienteResponseDto> UpdateAsync(UpdateClienteDto dto)
        {

            var cliente = await _clienteRepository.GetByIdAsync(dto.Id);
            if (cliente == null)
            {
                throw new Exception("El cliente no fue encontrado");
            }

            cliente.Nombre = dto.Nombre;
            cliente.Email = dto.Correo;
            cliente.Telefono = dto.Telefono;
            cliente.Cedula = dto.Cedula;
            cliente.Direccion = dto.Direccion;
            cliente.Estado = EstadoCliente.Activo;

            await _clienteRepository.UpdateAsync(cliente);

            var clienteResponsedto = new ClienteResponseDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Cedula = cliente.Cedula,
                Direccion = cliente.Direccion,
                estado = cliente.Estado
            };

            return clienteResponsedto;

        }

    }
}