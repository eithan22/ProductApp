
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApp.Aplication.BusinessValidator.Modulo_Usuarios;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.RulesBusinnes;
using ProductApp.Aplication.Mappers;
using ProductApp.Aplication.Services;
using ProductApp.Aplication.Validators.Modulo_Usuario.ClienteValidator;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

           //controllers
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            //base de datos

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
                    );
            });


            // repositorios
           // builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

            // mappers
            builder.Services.AddScoped<IMapperCliente, ClienteMappers>();
           // builder.Services.AddScoped<IMapperUsuario, UsuarioMapper>();


            // servicios
           // builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            builder.Services.AddScoped<IClienteServices, ClienteServices>();


            // reglas de negocio
            builder.Services.AddScoped<IValidatorBusinessClientes, ValidatorBusinessClientes>();


            // validadores

            builder.Services.AddScoped<IValidator<CreateClienteDto>, CreateClienteValidator>();
            builder.Services.AddScoped<IValidator<UpdateClienteDto>, UpdateClienteValidator>();
          




            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
