
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
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
    [JsonPropertyName("latestVersion"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string LatestVersion { get; set; }
    [JsonPropertyName("documentClass"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string DocumentClass { get; set; }
    [JsonPropertyName("type"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string Type { get; set; }
    //public required List<string> Categories { get; set; }
    [JsonPropertyName("lastModifiedAt"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastPublishedAt { get; set; }
    [NotMapped]
    [JsonPropertyName("latestCategories"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<LatestCategory>? LatestCategories { get; set; }
    public string? Categories { get; set; }
    [JsonPropertyName("documentUrl"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DocumentUrl { get; set; }
    [NotMapped]
    public string? HeaderContent { get; set; }
    [NotMapped]
    public string? FooterContent { get; set; }
    [NotMapped]
    public string? BodyContent { get; set; }
    [NotMapped]
    [JsonPropertyName("fullContentHtml"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FullContentHtml
    {
        get
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(HeaderContent)) sb.AppendLine($"<header>{HeaderContent}</header>");
            if (!string.IsNullOrWhiteSpace(BodyContent)) sb.AppendLine($"<main>{BodyContent}</main>");
            if (!string.IsNullOrWhiteSpace(FooterContent)) sb.AppendLine($"<footer>{FooterContent}</footer>");
            return sb.ToString();
        }
    }

    public Properties AsExternalItemProperties()
    {
        _ = DocumentId ?? throw new MemberAccessException("Id cannot be null");
        _ = LatestTitle ?? throw new MemberAccessException("Title cannot be null");
        // _ = Categories ?? throw new MemberAccessException("Categories cannot be null");

#pragma warning disable CS8604 // Possible null reference argument.
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
                { "documentUrl", DocumentUrl },
                { "content", FullContentHtml },
            }
        };
#pragma warning restore CS8604 // Possible null reference argument.

        return properties;
    }
}