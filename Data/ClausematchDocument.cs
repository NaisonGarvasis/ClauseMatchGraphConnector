
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ClauseMatchGraphConnector.ClausematchApiClient.Models;
using Microsoft.Graph.Models.ExternalConnectors;

namespace ClauseMatchGraphConnector.Data;

public class ClausematchDocument
{
    [JsonPropertyName("categories@odata.type")]
    private const string CategoriesODataType = "Collection(String)";

    [Key]
    [JsonPropertyName("id")]
    public required string DocumentId { get; set; }
    [JsonPropertyName("latestTitle")]
    public required string LatestTitle { get; set; }
    [JsonPropertyName("latestVersion")]
    public required string LatestVersion { get; set; }
    [JsonPropertyName("documentClass")]
    public required string DocumentClass { get; set; }
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    //public required List<string> Categories { get; set; }
    [JsonPropertyName("lastPublishedAt")]
    public required string LastPublishedAt { get; set; }
    [JsonPropertyName("latestCategories")]
    public IList<LatestCategory> LatestCategories { get; set; }
    public string Categories { get; set; }
    public string DocumentUrl { get; set; }


    public Properties AsExternalItemProperties()
    {
        _ = DocumentId ?? throw new MemberAccessException("Id cannot be null");
        _ = LatestTitle ?? throw new MemberAccessException("Title cannot be null");
       // _ = Categories ?? throw new MemberAccessException("Categories cannot be null");

        var properties = new Properties
        {
            AdditionalData = new Dictionary<string, object>
            {
                { "id", DocumentId },
                { "latestTitle", LatestTitle },
                { "latestVersion", LatestVersion },
                { "documentClass", DocumentClass },
                { "type", Type },
                { "lastPublishedAt", LastPublishedAt },
               // { "categories@odata.type", "Collection(String)" }
                { "categories", Categories },
                { "documentUrl", DocumentUrl }
            }
        };

        return properties;
    }
}