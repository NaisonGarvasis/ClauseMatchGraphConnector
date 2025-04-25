using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Models
{
    public class LatestCategory
    {
        [Key]
        [JsonPropertyName("categoryId")]
        public required string CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public required string CategoryName { get; set; }

    }
}
