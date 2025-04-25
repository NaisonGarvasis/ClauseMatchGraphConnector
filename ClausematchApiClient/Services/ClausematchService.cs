using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

using ClauseMatchGraphConnector.ClausematchApiClient.Models;
using ClauseMatchGraphConnector.Data;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Services
{
    public class ClausematchService : IClausematchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint = "https://api.clausematch.com/api/v1/categories";

        public ClausematchService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<List<Category>> GetAllCategoriesAsync(string jwtToken)
        {
            try
            {
                var categories = new List<Category>();
                int page = 1, size = 50;

                while (true)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoint}?page_number={page}&page_size={size}");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var stream = await response.Content.ReadAsStreamAsync();
                    var data = await JsonSerializer.DeserializeAsync<CategoryResponse>(stream);

                    if (data?.Categories == null || data.Categories.Count == 0)
                        break;

                    categories.AddRange(data.Categories);

                    if (data.CurrentPage >= data.TotalPages)
                        break;

                    page++;
                }

                return categories;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching categories: {ex.Message}", ex);
            }
        }

        public async Task<List<ClausematchDocument>> GetAllDocumentsByCategoryAsync(string jwtToken, string categoryId, Settings settings)
        {
            try
            {
                var documents = new List<ClausematchDocument>();
                int page = 1, size = 50;

                while (true)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoint}?page_number={page}&page_size={size}");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var stream = await response.Content.ReadAsStreamAsync();
                    var data = await JsonSerializer.DeserializeAsync<DocumentSearchResponse>(stream);

                    if (data?.Documents == null || data.Documents.Count == 0)
                        break;
                    documents.AddRange(data.Documents);
                    foreach (var doc in documents)
                    {
                        doc.Categories = string.Join(", ", doc.LatestCategories.Select(c => c.CategoryName));
                        doc.DocumentUrl = settings.ClausematchDocumentUrl + doc.DocumentId;
                    }

                    if (data.CurrentPage >= data.TotalPages)
                        break;

                    page++;
                }
                return documents;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching categories: {ex.Message}", ex);
            }
        }
    }
}


