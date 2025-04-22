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
        private readonly string _authEndpoint = "https://api.clausematch.com/auth/token";
        private readonly string _clientSecret = "your-client-secret";

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> GetJwtTokenAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, _authEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _clientSecret);

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
