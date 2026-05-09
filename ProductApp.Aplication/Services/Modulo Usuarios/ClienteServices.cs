using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.RulesBusinnes;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.ClientModel.Primitives;
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


        //falta hacer el bussenes aqui 
        public async Task<OperationResultD<List<ClienteResponseDto>>> BuscarAsync(string? nombre, string? telefono, string? correo)
        {
            
                if (string.IsNullOrEmpty(nombre) && string.IsNullOrEmpty(telefono) && string.IsNullOrEmpty(correo))
                {
                    return OperationResultD<List<ClienteResponseDto>>.Failure("Debe proporcionar al menos un criterio de búsqueda");
                }


                var clientes = await _clienteRepository.BuscarAsync(nombre, telefono, correo);

                var clienteresponsedto = clientes.
                    Select(c => _mapperCliente.MapToClienteResponseDto(c))
                    .ToList();

                return OperationResultD<List<ClienteResponseDto>>.Success(clienteresponsedto, "Clientes obtenidos correctamente");


            }
           
            
           

        




        public async Task<OperationResultD<ClienteResponseDto>> CreateAsync(CreateClienteDto dto)
        {
            // Validar el DTO utilizando FluentValidation
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ClienteResponseDto>.Failure($"Error de validación: {errors}");
            }

            //validar reglas de negocio
            var businessValidationResult = await _validatorBusinessClientes.ValidarCreateClienteAsync(dto);
            if (!businessValidationResult.IsSuccess)
            {
                return OperationResultD<ClienteResponseDto>.Failure(businessValidationResult.Message);
            }

            // Mapear el DTO a la entidad Cliente
            var cliente = _mapperCliente.MapToCreateCliente(dto);


            await _clienteRepository.CreateAsync(cliente);

            // Mapear la entidad Cliente a ClienteResponseDto
            var clienteresponse = _mapperCliente.MapToClienteResponseDto(cliente);

            return OperationResultD<ClienteResponseDto>.Success(clienteresponse);

        }


        //delete fisico
        public async Task<OperationResultD<bool>> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor o igual a 0");
            }

            var cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null)
            {
                return OperationResultD<bool>.Failure("El cliente no fue encontrado");

            }


            await _clienteRepository.DeleteAsync(id);

            return OperationResultD<bool>.Success(true);




        }

        //delete logico
        public async Task<OperationResultD<bool>> DisableAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor o igual a 0");
            }


            var cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null)
            {
                return OperationResultD<bool>.Failure("El cliente no fue encontrado");
            }

            var validationResult = await _validatorBusinessClientes.ValidarDeleteClienteAsync(cliente);
            if (!validationResult.IsSuccess)
            {
                return OperationResultD<bool>.Failure(validationResult.Message);
            }

            cliente.Desactivar();

            await _clienteRepository.UpdateAsync(cliente);
            return OperationResultD<bool>.Success(true, "Cliente desactivado correctamente");

            

        }



        public async Task<OperationResultD<List<ClienteResponseDto>>> GetAllAsync()
        {
            var clientes = await _clienteRepository.GetAllAsync();

            if (clientes == null || !clientes.Any())
            {
                return OperationResultD<List<ClienteResponseDto>>
                   .Success(new List<ClienteResponseDto>(), "No hay clientes registrados");
            }


            var clienteresponsedto = clientes.Select(c => _mapperCliente.MapToClienteResponseDto(c))
                .ToList();

            return OperationResultD<List<ClienteResponseDto>>.Success(clienteresponsedto, "Clientes obtenidos correctamente");



        }

        public async Task<OperationResultD<ClienteResponseDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<ClienteResponseDto>.Failure("El id no puede ser menor o igual a 0");
            }

            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                return OperationResultD<ClienteResponseDto>.Failure("El cliente no fue encontrado");
            }

            var clienteResponsedto = _mapperCliente.MapToClienteResponseDto(cliente);

            return OperationResultD<ClienteResponseDto>.Success(clienteResponsedto);

        }



        public async Task<OperationResultD<ClienteResponseDto>> UpdateAsync(UpdateClienteDto dto)
        {
            // Validar el DTO utilizando FluentValidation
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ClienteResponseDto>.Failure($"Error de validación: {errors}");
            }


            var cliente = await _clienteRepository.GetByIdAsync(dto.Id);
            if (cliente == null)
            {
                return OperationResultD<ClienteResponseDto>.Failure("El cliente no fue encontrado");
            }

             var validationResultBusiness = await _validatorBusinessClientes.ValidarUpdateClienteAsync(dto, cliente);

            if (!validationResultBusiness.IsSuccess)
            {
                return OperationResultD<ClienteResponseDto>.Failure(validationResultBusiness.Message);
            }



            _mapperCliente.MapToUpdateCliente(dto, cliente);

            await _clienteRepository.UpdateAsync(cliente);

            var clienteResponsedto = _mapperCliente.MapToClienteResponseDto(cliente);

            return OperationResultD<ClienteResponseDto>.Success(clienteResponsedto);

        }

    }
}