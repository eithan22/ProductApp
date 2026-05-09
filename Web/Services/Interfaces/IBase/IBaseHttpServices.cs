using ProductApp.Aplication.Result;

namespace Web.Services.Interfaces.IBase
{
    public interface IBaseHttpServices
    {
        Task<TResponse> GetAsync<TResponse>(string url);

        Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data);

        Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest data);

        Task<bool>DeleteAsync(string url);


    }
}

