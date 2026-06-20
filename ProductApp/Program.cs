
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProductApp.Aplication.BusinessValidator.Modulo_Productos;
using ProductApp.Aplication.BusinessValidator.Modulo_Usuarios;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Aplication.Interface.RulesBusinnes;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Mappers;
using ProductApp.Aplication.Mappers.Modulo_Producto;
using ProductApp.Aplication.Mappers.Modulo_Ventas;
using ProductApp.Aplication.Services;
using ProductApp.Aplication.Services.Modulo_Usuarios;
using ProductApp.Aplication.Validators.Modulo_Producto.CategoriaValidator;
using ProductApp.Aplication.Validators.Modulo_Producto.InventarioValidator;
using ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator;
using ProductApp.Aplication.Validators.Modulo_Usuario.AuthValidator;
using ProductApp.Aplication.Validators.Modulo_Usuario.ClienteValidator;
using ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.DetalleOrdenValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.OrdenValidator;
using ProductApp.Aplication.Validators.Modulo_Ventas.PagoValidator;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository;
using System.Text;


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


            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Pon aquí el token así: Bearer {tu_token}"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
     {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });




            

            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });










            //base de datos

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
                    );
            });


            // repositorios
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

            builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();

            builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

            builder.Services.AddScoped<IInventarioRepository, InventarioRepository>();

            builder.Services.AddScoped<IOrdenRepository, OrdenRepository>();

            builder.Services.AddScoped<IDetalleOrdenRepository, DetalleOrdenRepository>();

            builder.Services.AddScoped<IPagoRepository, PagoRepository>();




            // mappers
            builder.Services.AddScoped<IMapperCliente, ClienteMappers>();

            builder.Services.AddScoped<IMapperCategoria, CategoriaMapper>();

             builder.Services.AddScoped<IMapperUsuario, UsuarioMapper>();

            builder.Services.AddScoped<IMapperProducto, ProductoMapper>();

            builder.Services.AddScoped<IMapperInventario, InventarioMapper>();

            builder.Services.AddScoped<IMapperOrden, OrdenMapper>();    

            builder.Services.AddScoped<IMapperDetalleOrdencs, OrdenDetalleMapper>();

            builder.Services.AddScoped<IMapperPago, PagoMapper>();



            // servicios

            //usuario
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();

            // cliente
            builder.Services.AddScoped<IClienteServices, ClienteServices>();

            //Categoria
            builder.Services.AddScoped<ICategoriaServices, CategoriaServices>();

            // auth

            builder.Services.AddScoped<IAuthService, AuthServicecs>();

            //Producto

            builder.Services.AddScoped<IProductoServices, ProductoServices>();

            //inventario
            builder.Services.AddScoped<IInventarioServices, InventarioService>();

            // orden
            builder.Services.AddScoped<IOrdenServices, OrderServices>();

            // detalle orden

            builder.Services.AddScoped<IDetalleOrdenServices, DetalleOrdenService>();

            // pago
            builder.Services.AddScoped<IPagoServices, PagoService>();





            // reglas de negocio

            //  Cliente
            builder.Services.AddScoped<IValidatorBusinessClientes, ValidatorBusinessClientes>();

            //categoria
            builder.Services.AddScoped<IValidatorBusinessCategoria, ValidatorBusinessCategoria>();


            //usuario
            builder.Services.AddScoped<IValitadorBusinessUsuario, IValidatorBusinessUsuarios>();

            // auth
            builder.Services.AddScoped<IValidatorBusinessAuth, ValidatorBusinessAuth>();

            //Producto
            builder.Services.AddScoped<IValidatorBusinessProducto, ValidatorBusinessProducto>();
            



            // validadores dto

            //Cliente

            builder.Services.AddScoped<IValidator<CreateClienteDto>, CreateClienteValidator>();
            builder.Services.AddScoped<IValidator<UpdateClienteDto>, UpdateClienteValidator>();

            //Categoria
            builder.Services.AddScoped<IValidator<CreateCategoriaDto>, CreateCategoriaValidator>();
            builder.Services.AddScoped<IValidator<UpdateCategoriaDto>, UpdateCategoriaValidator>();


            //usuario
            builder.Services.AddScoped<IValidator<CreateUsuarioDto>, CreateUsuarioValidator>();
            builder.Services.AddScoped<IValidator<UpdateUsuarioDto>, UpdateUsuarioValidator>();
            builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordUsuarioValidator>();
             builder.Services.AddScoped<IValidator<CambiarRolDto>, CambiarRolUsuarioValidator>();

            // auth
            builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
            builder.Services.AddScoped<IValidator<RegisteDto>, RegisterValidator>();


            //Producto
            builder.Services.AddScoped<IValidator<CreateProductoDto>, CreateProductoValidator>();
            builder.Services.AddScoped<IValidator<UpdateProductoDto>, UpdateProductoValidator>();


            //Inventario
            builder.Services.AddScoped<IValidator<MovimientoStockDto>, MovimientoStockValidator>();
            builder.Services.AddScoped<IValidator<AjustarStockDto>, AjustarStockValidator>();

            // orden
            builder.Services.AddScoped<IValidator<CreateOrdenDto>, CreateOrdenValidator>();

            // detalle orden
            builder.Services.AddScoped<IValidator<CreateDetalleOrdenDto>, CreateDetalleOredenValidator>();
            builder.Services.AddScoped<IValidator<UpdateDetalleOrdenDto>, UpdateDetalleOrdenValidator>();

            // pago
            builder.Services.AddScoped<IValidator<CreatePagoDto>, CreatePagoValidator>();
















            var app = builder.Build();

            


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
