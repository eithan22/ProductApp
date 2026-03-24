using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.RulesBusinnes;
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
        private readonly IMapperCliente _mapperCliente;
        private readonly IValidator<CreateClienteDto> _createValidator;
        private readonly IValidator<UpdateClienteDto> _updateValidator;
        private readonly IValidatorBusinessClientes _validatorBusinessClientes;

        public ClienteServices(IClienteRepository clienterepository,
            IMapperCliente mapperCliente,
            IValidator<UpdateClienteDto> updateValidator,
            IValidator<CreateClienteDto> createValidator,
            IValidatorBusinessClientes validatorBusinessClientes


            )
        {
            _clienteRepository = clienterepository;
            _mapperCliente = mapperCliente;
            _updateValidator = updateValidator;
            _createValidator = createValidator;
            _validatorBusinessClientes = validatorBusinessClientes;
        }

        public async Task<List<ClienteResponseDto>> BuscarAsync(string? nombre, string? telefono, string? correo)
        {
           if (string.IsNullOrEmpty(nombre) && string.IsNullOrEmpty(telefono) && string.IsNullOrEmpty(correo))
            {
                throw new Exception("Debe proporcionar al menos un criterio de búsqueda");
            }


            var clientes = await _clienteRepository.BuscarAsync(nombre, telefono, correo);
           
            var clienteresponsedto = clientes.
                Select(c => _mapperCliente.MapToClienteResponseDto(c))
                .ToList();

            return(clienteresponsedto);

        }




        public async Task<ClienteResponseDto> CreateAsync(CreateClienteDto dto)
        {
            // Validar el DTO utilizando FluentValidation
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Exception($"Error de validación: {errors}");
            }

            //validar reglas de negocio
            await  _validatorBusinessClientes.ValidarCreateClienteAsync(dto);

            // Mapear el DTO a la entidad Cliente
            var cliente = _mapperCliente.MapToCreateCliente(dto);

            await _clienteRepository.CreateAsync(cliente);

            // Mapear la entidad Cliente a ClienteResponseDto
            var clienteresponse = _mapperCliente.MapToClienteResponseDto(cliente);

            return clienteresponse;

        }


        //delete fisico
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

        //delete logico
        public async Task DisableAsync(int id)
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
            await _validatorBusinessClientes.ValidarDeleteClienteAsync(cliente);

            cliente.Desactivar();

            await _clienteRepository.UpdateAsync(cliente);

        }



        public async Task<List<ClienteResponseDto>> GetAllAsync()
        {
            var clientes = await _clienteRepository.GetAllAsync();

            var clienteresponsedto = clientes.Select(c => _mapperCliente.MapToClienteResponseDto(c))
                .ToList();

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

            var clienteResponsedto = _mapperCliente.MapToClienteResponseDto(cliente);

            return clienteResponsedto;

        }



        public async Task<ClienteResponseDto> UpdateAsync(UpdateClienteDto dto)
        {
            // Validar el DTO utilizando FluentValidation
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Exception($"Error de validación: {errors}");
            }


            var cliente = await _clienteRepository.GetByIdAsync(dto.Id);
            if (cliente == null)
            {
                throw new Exception("El cliente no fue encontrado");
            }

              await _validatorBusinessClientes.ValidarUpdateClienteAsync(dto, cliente);

            _mapperCliente.MapToUpdateCliente(dto, cliente);

            await _clienteRepository.UpdateAsync(cliente);

            var clienteResponsedto = _mapperCliente.MapToClienteResponseDto(cliente);

            return clienteResponsedto;

        }

    }
}