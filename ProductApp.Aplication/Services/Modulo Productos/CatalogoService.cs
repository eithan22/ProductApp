using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Interface;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class CatalogoService : ICategoriaServices
    {
        private readonly ICategoriaRepository _categoriaRepository;
        public CatalogoService(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }


        public async Task<CategoriaResponseDto> CreateAsync(CreateCategoriaDto dto)
        {
            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            await _categoriaRepository.CreateAsync(categoria);

            var categoriaResponsedto = new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };

            return categoriaResponsedto;
        }



        public async Task DeleteAsync(int id)
        {
            if(id <=0)
            {
                throw new Exception("El id no puede ser menor o igual a 0");
            }
             var categoria =  _categoriaRepository.GetByIdAsync(id);
            if(categoria == null)
            {
                throw new Exception("La categoria no fue encontrada");
            }
             await _categoriaRepository.DeleteAsync(id);
        }


        public async Task DisableAsync(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El id no puede ser menor o igual a 0");
            }
             var categoria = await _categoriaRepository.GetByIdAsync(id);

            if (categoria == null)
            {
                throw new Exception("La categoria no fue encontrada");
            }
             await _categoriaRepository.DisebleAsync(id);

        }

        public async Task<List<CategoriaResponseDto>> GetAllAsync()
        {
            var categorias = await _categoriaRepository.GetAllAsync();

            var categoriaresponsedto = categorias
                .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion
            }).ToList();

            return categoriaresponsedto;
        }



        public async Task<CategoriaResponseDto> GetByIdAsync(int id)
        {
           if(id <= 0)
             throw new Exception("El id no puede ser menor o igual a 0");

            
             var categoria =  await _categoriaRepository.GetByIdAsync(id);
           
            if(categoria == null)
            {
                throw new Exception("La categoria no fue encontrada");
            }
            var categoriaresponsedto = new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };
            return (categoriaresponsedto);
        }

        public async Task<CategoriaResponseDto> UpdateAsync(UpdateCategoriaDto dto)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(dto.Id);
            if (categoria == null)
            {
                throw new Exception("La categoria no fue encontrada");
            }

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;

            await _categoriaRepository.UpdateAsync(categoria);

            var categoriaResponsedto = new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };

            return categoriaResponsedto;

        }
    }
}
