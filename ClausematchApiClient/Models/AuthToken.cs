
using System.Text.Json.Serialization;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Models
{
    public class AuthToken
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }
    }
}
