using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;

using ClauseMatchGraphConnector.ClausematchApiClient.Models;
using ClauseMatchGraphConnector.Data;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Services
{
    public class ClausematchService : IClausematchService
    {
        private readonly HttpClient _httpClient;
        public ClausematchService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<List<Category>> GetAllCategoriesAsync(string jwtToken, Settings settings)
        {
            try
            {
                var categories = new List<Category>();
                int page = 1, size = 50;

                while (true)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.ClausematchApiBaseUrl}/categories?pageNumber={page}&pageSize={size}");
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
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.ClausematchApiBaseUrl}/documents/search?categoryId={categoryId}&pageNumber={page}&pageSize={size}");
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
#pragma warning disable CS8604 // Possible null reference argument.
                        doc.Categories = string.Join(", ", doc.LatestCategories.Select(c => c.CategoryName));
#pragma warning restore CS8604 // Possible null reference argument.
                        doc.DocumentUrl = settings.ClausematchDocumentUrl + doc.DocumentId;

                        DocumentContent content = await GetDocumentContentByIdAsync(jwtToken, doc.DocumentId, doc.LatestVersion, settings);

                        doc.HeaderContent = content?.Header?.Content;
                        doc.FooterContent = content?.Footer?.Content;
                        doc.BodyContent = content?.Body != null
                            ? string.Join("\n", content.Body.Select(b => b.Content))
                            : string.Empty;
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

        public async Task<DocumentContent> GetDocumentContentByIdAsync(string jwtToken, string documentId, string version, Settings settings)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.ClausematchApiBaseUrl}/documents/{documentId}/versions/{version}/paragraphs");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"[ERROR] Failed to get content for document '{documentId}' version '{version}': {response.StatusCode}");
                }
                var json = await response.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var content = JsonSerializer.Deserialize<DocumentContent>(json, options);
                return content ?? new DocumentContent();
            }
            catch (Exception ex)
            {
                throw new Exception($"[EXCEPTION] Error retrieving document content for '{documentId}': {ex.Message}");
            }
        }
    }
}


