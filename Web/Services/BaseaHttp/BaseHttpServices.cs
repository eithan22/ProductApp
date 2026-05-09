using Azure;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using ProductApp.Aplication.Result.ApiResponses;
using System.Net.Http;
using System.Text.Json;
using Web.Services.Interfaces.IBase;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Web.Services.Base
{
    public class BaseHttpServices : IBaseHttpServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;// Inyectamos IHttpContextAccessor para acceder al contexto HTTP actual

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BaseHttpServices(IHttpClientFactory httpClientFactory
            ,IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("BaseUrl");

            // 🔐 obtener token desde sesión
            var token = _httpContextAccessor.HttpContext.Session.GetString("TOKEN");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // 🔹 GET
        public async Task<TResponse> GetAsync<TResponse>(string url)
        {
            var client = CreateClient();

            var response = await client.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception($"Respuesta vacía. Status: {response.StatusCode}");
            }

            // ✅ 1. Intentar leer el mensaje de la API SIEMPRE
            var apiResponse = JsonSerializer.Deserialize<ApiResponseT<TResponse>>(content, _jsonOptions);

            if (apiResponse == null)
                throw new Exception("Error al deserializar la respuesta");

            // ✅ 2. Si HTTP falla → mostrar mensaje limpio
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(string.IsNullOrWhiteSpace(apiResponse.Message)
                    ? $"Error HTTP {response.StatusCode}"
                    : apiResponse.Message);
            }

            // ✅ 3. Validar negocio
            if (!apiResponse.Success)
            {
                throw new Exception(string.IsNullOrWhiteSpace(apiResponse.Message)
                    ? "Error en la API sin mensaje"
                    : apiResponse.Message);
            }

            // ✅ 4. Retornar
            return apiResponse.Data!;

        }

        // 🔹 POST
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            var client = CreateClient();

            var response = await client.PostAsJsonAsync(url, data);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception($"Respuesta vacía. Status: {response.StatusCode}");
            }

            // ✅ 1. Intentar leer el mensaje de la API SIEMPRE
            var apiResponse = JsonSerializer.Deserialize<ApiResponseT<TResponse>>(content, _jsonOptions);

            if (apiResponse == null)
                throw new Exception("Error al deserializar la respuesta");

            // ✅ 2. Si HTTP falla → mostrar mensaje limpio
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(string.IsNullOrWhiteSpace(apiResponse.Message)
                    ? $"Error HTTP {response.StatusCode}"
                    : apiResponse.Message);
            }

            // ✅ 3. Validar negocio
            if (!apiResponse.Success)
            {
                throw new Exception(string.IsNullOrWhiteSpace(apiResponse.Message)
                    ? "Error en la API sin mensaje"
                    : apiResponse.Message);
            }

            // ✅ 4. Retornar
            return apiResponse.Data!;







        }

        // 🔹 PUT
        public async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            var client = CreateClient();

            var response = await client.PutAsJsonAsync(url, data);

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception($"Respuesta vacía. Status: {response.StatusCode}");
            }

            // ✅ 1. Intentar leer el mensaje de la API SIEMPRE
            var apiResponse = JsonSerializer.Deserialize<ApiResponseT<TResponse>>(content, _jsonOptions);

            if (apiResponse == null)
                throw new Exception("Error al deserializar la respuesta");

            // ✅ 2. Si HTTP falla → mostrar mensaje limpio
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(string.IsNullOrWhiteSpace(apiResponse.Message)
                    ? $"Error HTTP {response.StatusCode}"
                    : apiResponse.Message);
            }

            // ✅ 3. Validar negocio
            if (!apiResponse.Success)
            {
                throw new Exception(string.IsNullOrWhiteSpace(apiResponse.Message)
                    ? "Error en la API sin mensaje"
                    : apiResponse.Message);
            }

            // ✅ 4. Retornar
            return apiResponse.Data!;




        }

        // 🔹 DELETE
        public async Task<bool> DeleteAsync(string url)
        {
            var client = CreateClient();

            var response = await client.DeleteAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception($"Respuesta vacía. Status: {response.StatusCode}");
            }

            // ✅ 1. Deserializar SIEMPRE
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);

            if (apiResponse == null)
                throw new Exception("Error al deserializar la respuesta");

            // ✅ 2. Validar HTTP con mensaje limpio
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(string.IsNullOrWhiteSpace(apiResponse.Message)
                    ? $"Error HTTP {response.StatusCode}"
                    : apiResponse.Message);
            }

            // ✅ 3. Validar negocio
            if (!apiResponse.Success)
            {
                throw new Exception(string.IsNullOrWhiteSpace(apiResponse.Message)
                    ? "Error en la API sin mensaje"
                    : apiResponse.Message);
            }

            // ✅ 4. Retornar true directo (más seguro)
            return true;
        }
    }

}