using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.IMappers.Modulos_Productos
{
    public interface IMapperInventario
    {
        InventarioResponseDto MapToInventarioResponse(Inventario inventario);
    }
}
