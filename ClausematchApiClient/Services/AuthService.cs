using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClauseMatchGraphConnector.ClausematchApiClient.Models;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> GetJwtTokenAsync(Settings settings)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, settings.ClausematchAuthEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ClausematchAuthKey);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseStream = await response.Content.ReadAsStreamAsync();
                var authToken = await JsonSerializer.DeserializeAsync<AuthToken>(responseStream);

                return authToken?.AccessToken ?? throw new Exception("Access token not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve token: {ex.Message}", ex);
            }
        }
    }
}
