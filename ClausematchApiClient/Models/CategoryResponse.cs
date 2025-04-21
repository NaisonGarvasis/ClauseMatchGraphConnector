
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Models
{
    public class CategoryResponse
    {
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("categories")]
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

