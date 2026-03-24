using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IOrdenServices :IBaseServices<OrdenResponseDto, CreateOrdenDto, UpdateOrdenDto>
    {

    }
}
