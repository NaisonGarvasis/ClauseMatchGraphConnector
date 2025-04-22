
using System.Text.Json.Serialization;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Models
{
    public class AuthToken
    {
        [JsonPropertyName("accessToken")]
        public required string AccessToken { get; set; }
    }
}
