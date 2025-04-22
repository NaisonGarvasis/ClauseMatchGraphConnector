using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Models
{
    public class DocumentSearchResponse
    {
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("resultsCount")]
        public int ResultsCount { get; set; }

        [JsonPropertyName("content")]
        public List<Document>? Documents { get; set; }
    }
    public class Document
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("documentClass")]
        public required string DocumentClass { get; set; }

        [JsonPropertyName("latestVersion")]
        public required string LatestVersion { get; set; }

        [JsonPropertyName("latestTitle")]
        public required string LatestTitle { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }
    }
}
