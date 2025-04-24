using ClauseMatchGraphConnector.ClausematchApiClient.Models;
using ClauseMatchGraphConnector.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClauseMatchGraphConnector.ClausematchApiClient
{
    public static class ClausematchDocumentGenerator
    {
        public static List<ClausematchDocument> GenerateDocuments(int count)
        {
            var documents = new List<ClausematchDocument>();

            for (int i = 1; i <= count; i++)
            {
                documents.Add(new ClausematchDocument
                {
                    DocumentId = Guid.NewGuid().ToString(),
                    DocumentClass = $"Class-{i}",
                    LatestVersion = $"v{1}.{i}",
                    LatestTitle = $"Sample Document Title {i}",
                    Type = (i % 2 == 0) ? "Policy" : "Procedure",
                    LastPublishedAt = DateTime.UtcNow.AddDays(-i - 2).ToString("o"),
                });
            }

            return documents;
        }
    }    
}
