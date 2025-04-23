// See https://aka.ms/new-console-template for more information
using ClauseMatchGraphConnector.ClausematchApiClient;
using ClauseMatchGraphConnector.ClausematchApiClient.Models;
using ClauseMatchGraphConnector.ClausematchApiClient.Services;
using ClauseMatchGraphConnector.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        // IList<ClausematchDocument> documents = GetClauseMatchDocumentsAsync(args).Result;
        IList<ClausematchDocument> documents = ClausematchDocumentGenerator.GenerateDocuments(50);

    }

    private static async Task<IList<ClausematchDocument>> GetClauseMatchDocumentsAsync(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();
        IList<ClausematchDocument> documents = new List<ClausematchDocument>();
        var authService = host.Services.GetRequiredService<IAuthService>();
        var clausematchService = host.Services.GetRequiredService<IClausematchService>();
        try
        {
            var token = await authService.GetJwtTokenAsync();
            var categories = await clausematchService.GetAllCategoriesAsync(token);
            foreach (var category in categories)
            {
                Console.WriteLine($"Category ID: {category.Id}, Name: {category.Name}");
                 documents = await clausematchService.GetAllDocumentsByCategoryAsync(token, category.Id);
                foreach (var document in documents)
                {
                    Console.WriteLine($"Document ID: {document.Id}, Latest Title: {document.LatestTitle}, Latest Version: {document.LatestVersion}");
                }
                break;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
        return documents;
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
                services.AddHttpClient()
                        .AddSingleton<IAuthService, AuthService>()
                        .AddSingleton<IClausematchService, ClausematchService>());
}
