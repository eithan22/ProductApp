using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Configuraciones;
using ProductApp.Infraesctructura.Persistencia.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class ProductoServices : IProductoServices
    {
        private readonly IProductoRepository _productorepository;

        public ProductoServices(IProductoRepository productorepository)
        {
            _productorepository = productorepository;
           
        }

        public async Task<OperationResultD<bool>> DeleteAsync(int id)
        {
            if (id >= 0)
            {
                throw new Exception("El id no puede ser menor que 0");
            }

            var result = await _productorepository.GetByIdAsync(id);

            if(result == null)
            {
                throw new Exception("El producto no fue encontrado");
            }

            await _productorepository.DeleteAsync(id);
            return OperationResultD<bool>.Success(true, "Producto eliminado exitosamente");



        }

        public async Task<OperationResultD<bool>> DisableAsync(int id)
        {
            var result = await _productorepository.GetByIdAsync(id);

            if (result == null)
            {
                throw new Exception("El producto no fue encontrado");

            }

            await _productorepository.DisebleAsync(id);
            return OperationResultD<bool>.Success(true, "Producto deshabilitado exitosamente");

        }

         public async Task<OperationResultD<ProductoResponseDto>> CreateAsync(CreateProductoDto dto)
        {
            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Costo = dto.Costo
         

            };

            await _productorepository.CreateAsync(producto);


            var productoresponsedto = new ProductoResponseDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Costo = producto.Costo,
                Estado = producto.Estado

            };

            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto creado correctamente");

              



        }

       public async Task<OperationResultD<List<ProductoResponseDto>>> GetAllAsync()
        {
            var productos = await _productorepository.GetAllAsync();

            var productoresponsedto = productos
                .Select(p => new ProductoResponseDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Costo = p.Costo,
                    Estado = p.Estado,
                    Categoria = p.Categoria // aun no 

                }).ToList();

               return OperationResultD<List<ProductoResponseDto>>.Success(productoresponsedto, "Productos obtenidos correctamente");
        }

        

        public async Task<OperationResultD<ProductoResponseDto>> GetByIdAsync(int id)
        {
            if (id >= 0)
            {
                throw new Exception("el id es invalido ");
            }

           var producto = await _productorepository.GetByIdAsync(id);

            if (producto == null)
            {
                throw new Exception("Producto no encontrado");

            }

            var productoresponsedto = new ProductoResponseDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Costo = producto.Costo,
                Estado = producto.Estado,
                Categoria = producto.Categoria
            };
            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto obtenido correctamente");



           
        }

         public async Task<OperationResultD<ProductoResponseDto>> UpdateAsync(UpdateProductoDto dto)
        {
           var producto = await _productorepository.GetByIdAsync(dto.Id);

            if (producto == null)
            {
                throw new Exception("producto no encontrado");
            }

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Costo = dto.costo;
            producto.Categoria = dto.categoria;
            producto.Estado = dto.Estado;

            await _productorepository.UpdateAsync(producto);

            var productoresponsedto = new ProductoResponseDto
            {
                Id = producto.Id,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Costo = producto.Costo,
                Estado = producto.Estado,
                Categoria = producto.Categoria
            };

            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto actualizado correctamente");
        }

      
    }
}
