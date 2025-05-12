using Microsoft.Extensions.Configuration;

namespace ClauseMatchGraphConnector;

public class Settings
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TenantId { get; set; }
    public string? ClausematchDocumentUrl { get; set; }
    public string? ClausematchAuthKey { get; set; }
    public string? ClausematchAuthEndpoint { get; set; }
    public string? ClausematchApiBaseUrl { get; set; }
    public string? IsAdminUser { get; set; }
    public string? DefaultClausematchGraphConnectionId { get; set; }
    public List<string> Categories { get; set; } = new();


    public static Settings LoadSettings()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) 
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        return config.GetRequiredSection("Settings").Get<Settings>() ??
            throw new Exception("Could not load app settings. See README for configuration instructions.");
    }
}
