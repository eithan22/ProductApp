using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.BaseServices
{
    public interface IBaseServices<TResponsedto , TCreatedto, TUpdatedto>
    {
        Task<TResponsedto> CreateAsync(TCreatedto dto);

        Task<List<TResponsedto>>GetAllAsync();

        Task<TResponsedto> GetByIdAsync(int id);

        Task<TResponsedto> UpdateAsync(TUpdatedto dto);

        Task DeleteAsync(int id);

        Task DisableAsync(int id);

        


    }
}
