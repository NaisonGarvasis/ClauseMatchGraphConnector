using ClauseMatchGraphConnector.ClausematchApiClient.Services;
using ClauseMatchGraphConnector.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClauseMatchGraphConnector.ClausematchApiClient
{
    public static class ApiClientOrchestrator
    {
        public async static Task<IList<ClausematchDocument>> GetClauseMatchDocumentsAsync(Settings settings)
        {
            var host = Host.CreateDefaultBuilder()
                       .ConfigureServices((context, services) =>
                       {
                           // Register IHttpClientFactory
                           services.AddHttpClient();
                           services.AddTransient<IAuthService, AuthService>();
                           services.AddTransient<IClausematchService, ClausematchService>();
                       })
                       .Build();
            List<ClausematchDocument> documents = new List<ClausematchDocument>();
            List<ClausematchDocument> distinctDocumentsList = new List<ClausematchDocument>();
            var authService = host.Services.GetRequiredService<IAuthService>();
            var clausematchService = host.Services.GetRequiredService<IClausematchService>();
            try
            {
                Console.WriteLine("Invoking Token Endpoint...");
                var token = await authService.GetJwtTokenAsync(settings);
                Console.WriteLine("Token Received" + token);
                Console.WriteLine("Invoking Categories Endpoint...");
                var categories = await clausematchService.GetAllCategoriesAsync(token, settings);
                Console.WriteLine("Total Categories" + categories.Count);

                // Normalize case for comparison
                var configuredCategories = settings.Categories
                    .Select(c => c.Trim().ToLowerInvariant())
                    .ToHashSet();
                var filteredCategories = categories
                    .Where(c => configuredCategories.Contains(c.Name.Trim().ToLowerInvariant()))
                    .ToList();
                Console.WriteLine("Filtered Categories: " + filteredCategories.Count);
                foreach (var category in filteredCategories)
                {
                    Console.WriteLine($"Processing Category: {category.Name} (ID: {category.Id})");
                    var documentesForCategory = await clausematchService.GetAllDocumentsByCategoryAsync(token, category.Id, settings);
                    documents.AddRange(documentesForCategory);
                }
                Console.WriteLine("Total Documents Fetched: " + documents.Count);
                distinctDocumentsList = documents.DistinctBy(x => x.DocumentId).ToList();
                Console.WriteLine("Total Distinct Documents: " + distinctDocumentsList.Count);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                throw new Exception($"Error fetching document info: {ex.Message}", ex);
            }
            return distinctDocumentsList;
        }

    }
}
