using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Entitis
{
    public class Cliente : BaseEntity
    {

        public string Nombre { get; private set; } = string.Empty;
        public string Cedula { get; private set; } = string.Empty;

        public string Direccion { get; private set; } = string.Empty;
        public string Correo { get; private set; } = string.Empty;
        public string Telefono { get; private set; } = string.Empty;

        public EstadoCliente Estado { get; private set; }

        public List<Orden> Ordenes { get; private set; } = new List<Orden>();


        public Cliente(string nombre, string cedula, string direccion, string correo, string telefono)
        {
                ValidarNombre(nombre);
                ValidarCedula(cedula);
                ValidarDireccion(direccion);
                ValidarCorreo(correo);
                ValidarTelefono(telefono);
               

            Nombre = nombre;
            Cedula = cedula;
            Direccion = direccion;
            Correo = correo;
            Telefono = telefono;

          Estado = EstadoCliente.Activo;

        }


        


        private void ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");
        }

        private void ValidarCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                throw new ArgumentException("La cédula no puede estar vacía.");
        }

        private void ValidarDireccion(string direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion))
                throw new ArgumentException("La dirección no puede estar vacía.");
        }

        private void ValidarCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("El email no puede estar vacío.");

            if (!correo.Contains("@"))
                throw new ArgumentException("El email no es válido.");
        }

        private void ValidarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                throw new ArgumentException("El teléfono no puede estar vacío.");

            if (telefono.Length != 10)
                throw new ArgumentException("El teléfono debe tener 10 dígitos.");
        }


        public void Desactivar()
        {
            if (Estado == EstadoCliente.Inactivo)
                throw new InvalidOperationException("El cliente ya está inactivo.");

            Estado = EstadoCliente.Inactivo;
        }

        public void Activar()
        {
            if (Estado == EstadoCliente.Activo)
                throw new InvalidOperationException("El cliente ya está activo.");
            Estado = EstadoCliente.Activo;
        }




        //actualizar

        public void CambiarYvalidarNombre(string nombre)
            {
                ValidarNombre(nombre);
                Nombre = nombre;
        }

        public void CambiarYvalidarCedula(string cedula)
        {
            ValidarCedula(cedula);
            Cedula = cedula;
        }

        public void CambiarYvalidarDireccion(string direccion)
        {
            ValidarDireccion(direccion);
            Direccion = direccion;
        }

        public void CambiarYvalidarCorreo(string correo)
        {
            ValidarCorreo(correo);
            Correo = correo;
        }

        public void CambiarYvalidarTelefono(string telefono)
        {
            ValidarTelefono(telefono);
            Telefono = telefono;
        }

        
    }

}