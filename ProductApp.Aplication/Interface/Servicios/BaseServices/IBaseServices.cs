using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.Servicios.BaseServices
{
    public interface IBaseServices<TResponsedto , TCreatedto, TUpdatedto>
    {
        Task<OperationResultD<TResponsedto>> CreateAsync(TCreatedto dto);

        Task<OperationResultD<List<TResponsedto>>> GetAllAsync();

        Task<OperationResultD<TResponsedto>> GetByIdAsync(int id);
        Task<OperationResultD<TResponsedto>> UpdateAsync(TUpdatedto dto);

        Task<OperationResultD<bool>> DeleteAsync(int id);

        Task<OperationResultD<bool>> DisableAsync(int id);

        


    }
}
