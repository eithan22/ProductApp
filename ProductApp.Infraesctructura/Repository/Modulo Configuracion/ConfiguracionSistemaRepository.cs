using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class ConfiguracionSistemaRepository : IConfiguracionSistemaRepository
    {
        private readonly AppDbContext _context;

        public ConfiguracionSistemaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConfiguracionSistema?> ObtenerAsync()
        {
            return await _context.ConfiguracionSistema.FirstOrDefaultAsync();
        }

        public async Task ActualizarAsync(ConfiguracionSistema configuracion)
        {
            _context.ConfiguracionSistema.Update(configuracion);
            await _context.SaveChangesAsync();
        }
    }
}
