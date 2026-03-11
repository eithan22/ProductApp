using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class DetalleOrdenRepository : GenericRepository<OrderDetalle>, IDetalleOrdenRepository
    {
        public DetalleOrdenRepository(AppDbContext context) : base(context)
        {
        }
    }
}
