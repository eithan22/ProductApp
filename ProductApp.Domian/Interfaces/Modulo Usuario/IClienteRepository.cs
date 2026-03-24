using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Interfaces
{
    public interface IClienteRepository : IGeneryRepository<Cliente>
    {

         Task <List<Cliente>>BuscarAsync(string? nombre, string? telefono, string? correo);
    }
}
