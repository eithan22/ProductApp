using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class CategoriaReposiroy : GenericRepository<Categoria>, ICategoriaRepository
    {
        public CategoriaReposiroy(AppDbContext context) : base(context)
        {
        }
    }
}
