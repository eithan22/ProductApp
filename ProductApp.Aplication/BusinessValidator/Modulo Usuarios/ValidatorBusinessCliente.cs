using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface.RulesBusinnes;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Usuarios
{
    public class ValidatorBusinessCliente : IValidatorBusinessClientes
    {
        private readonly IClienteRepository _clienteRepository;

        public ValidatorBusinessCliente(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task ValidarCreateClienteAsync(CreateClienteDto dto)
        {
            if (await _clienteRepository.ExisteAsync(c => c.Correo == dto.Correo))
            {
                throw new Exception("El correo ya está registrado");
            }

               if (await _clienteRepository.ExisteAsync(c => c.Cedula == dto.Cedula))
                {
                    throw new Exception("La cédula ya está registrada");
            }

            if (await _clienteRepository.ExisteAsync(c => c.Telefono == dto.Telefono))
            {
                throw new Exception("El teléfono ya está registrado");

            }

            //dudoso ponerlo porque el nombre puede ser igual a otro cliente, pero lo dejo para que no se repita el mismo nombre
            if (await _clienteRepository.ExisteAsync(c => c.Nombre == dto.Nombre))
            {
                throw new Exception("El nombre ya está registrado");

            }

            

              





            }
           
        public async Task ValidarDeleteClienteAsync(Cliente cliente)
        {
          

            if (cliente.Estado == EstadoCliente.Inactivo)
            {
                throw new Exception("El cliente ya está inactivo");
            }

  
         
        }

       

        public async Task ValidarUpdateClienteAsync(UpdateClienteDto dto, Cliente cliente)
        {
            if (cliente.Estado == EstadoCliente.Inactivo)
            {
                throw new Exception("No se puede actualizar un cliente inactivo");
            }

             if (cliente.Correo != dto.Correo && _clienteRepository.ExisteAsync(c => c.Correo == dto.Correo).Result)
            {
                throw new Exception("El correo ya está registrado por otro cliente");
            }

             if (cliente.Cedula != dto.Cedula && _clienteRepository.ExisteAsync(c => c.Cedula == dto.Cedula).Result)
                {
                    throw new Exception("La cédula ya está registrada por otro cliente");
                }
    
                if (cliente.Telefono != dto.Telefono && _clienteRepository.ExisteAsync(c => c.Telefono == dto.Telefono).Result)
                {
                    throw new Exception("El teléfono ya está registrado por otro cliente");
                }
    
                if (cliente.Nombre != dto.Nombre && _clienteRepository.ExisteAsync(c => c.Nombre == dto.Nombre).Result)
                {
                    throw new Exception("El nombre ya está registrado por otro cliente");
            }
        }
    }
}
