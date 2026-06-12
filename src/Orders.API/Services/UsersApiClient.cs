namespace Orders.API.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IUsersApiClient
    {
        Task<bool> UserExistsAsync(Guid userId);
    }

    public class UsersApiClient : IUsersApiClient
    {
        private readonly HttpClient _httpClient;

        public UsersApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // IMPORTANTE: Asegurate de poner el puerto exacto en el que corre tu Users.API en tu compu
            _httpClient.BaseAddress = new Uri("https://localhost:7003/");
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            
            var response = await _httpClient.GetAsync($"api/users/{userId}");

            
            return response.IsSuccessStatusCode;
        }
    }
}
