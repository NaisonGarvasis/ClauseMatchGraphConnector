
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Models
{
    public class CategoryResponse
    {
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("content")]
        public List<Category>? Categories { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        // Add other relevant properties
    }
}

