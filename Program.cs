// See https://aka.ms/new-console-template for more information
using ClauseMatchGraphConnector.ClausematchApiClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();

        var authService = host.Services.GetRequiredService<IAuthService>();
        var clausematchService = host.Services.GetRequiredService<IClausematchService>();

        try
        {
            var token = await authService.GetJwtTokenAsync();
            var categories = await clausematchService.GetAllCategoriesAsync(token);
            foreach (var category in categories)
            {
                Console.WriteLine($"Category ID: {category.Id}, Name: {category.Name}");
                var documents = await clausematchService.GetAllDocumentsByCategoryAsync(token, category.Id);
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
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
                services.AddHttpClient()
                        .AddSingleton<IAuthService, AuthService>()
                        .AddSingleton<IClausematchService, ClausematchService>());
}
