using ClauseMatchGraphConnector;
using ClauseMatchGraphConnector.ClausematchApiClient;
using ClauseMatchGraphConnector.Data;
using ClauseMatchGraphConnector.Graph;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Models.ODataErrors;
using System.Text.Json;
using Serilog;

// Initialize Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/app-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
ExternalConnection? currentConnection = null;

try
{
    Console.WriteLine("Clausematch documents Search Connector\n");
    Log.Information("Application started");
    var settings = Settings.LoadSettings();

    // Initialize Graph
    InitializeGraph(settings);

    if (settings.IsAdminUser == "true")
    {
        int choice = -1;
        while (choice != 0)
        {
            Console.WriteLine($"Current connection: {(currentConnection == null ? "NONE" : currentConnection.Name)}\n");
            Console.WriteLine("Please choose one of the following options:");
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Create a connection");
            Console.WriteLine("2. Select an existing connection");
            Console.WriteLine("3. Delete current connection");
            Console.WriteLine("4. Register schema for current connection");
            Console.WriteLine("5. Update schema for current connection");
            Console.WriteLine("6. View schema for current connection");
            Console.WriteLine("7. Push updated items to current connection");
            Console.WriteLine("8. Push ALL items to current connection");
            Console.WriteLine("9. Verify Clausematch API Connectivity");
            Console.WriteLine("10. Delete all items in current connection");
            Console.Write("Selection: ");

            try
            {
                choice = int.Parse(Console.ReadLine() ?? string.Empty);
            }
            catch (FormatException)
            {
                choice = -1;
            }

            switch (choice)
            {
                case 0:
                    // Exit the program
                    Console.WriteLine("Goodbye...");
                    Log.Information("Application exiting");
                    break;
                case 1:
                    currentConnection = await CreateConnectionAsync();
                    break;
                case 2:
                    currentConnection = await SelectExistingConnectionAsync();
                    break;
                case 3:
                    await DeleteCurrentConnectionAsync(currentConnection);
                    currentConnection = null;
                    break;
                case 4:
                    await RegisterSchemaAsync(false);
                    break;
                case 5:
                    await RegisterSchemaAsync(true);
                    break;
                case 6:
                    await GetSchemaAsync();
                    break;
                case 7:
                    await UpdateItemsFromDatabaseAsync(true, settings.TenantId);
                    break;
                case 8:
                    await UpdateItemsFromDatabaseAsync(false, settings.TenantId);
                    break;
                case 9:
                    var documents = await ApiClientOrchestrator.GetClauseMatchDocumentsAsync(settings);
                    string jsonString = JsonSerializer.Serialize(documents, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(jsonString);
                    break;
                case 10:
                    using (var db = new ClausematchDbContext())
                    {
                        db.MarkAllDocumentsAsDeleted();
                        Console.WriteLine("All documents marked as deleted in local DB.");
                    }
                    await DeleteAllDeletedItemsFromDatabaseAsync(settings.TenantId);
                    break;

                default:
                    Console.WriteLine("Invalid choice! Please try again.");
                    break;
            }
        }
    }
    else
    {
        Console.WriteLine("Pushing items to current connection...");
        Log.Information("Pushing items to current connection...");
        var connections = await GraphHelper.GetExistingConnectionsAsync();
        currentConnection = connections?.Value?.FirstOrDefault(c => c.Id == settings.DefaultClausematchGraphConnectionId);

        if (currentConnection == null)
        {
            Console.WriteLine($"No connection found with ID: {settings.DefaultClausematchGraphConnectionId}");
            return;
        }
        await UpdateItemsFromDatabaseAsync(false, settings.TenantId);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unhandled exception: {ex}");
    Log.Error(ex, "Unhandled exception occurred");
}
finally
{
    Log.Information("Application shutting down");
    Log.CloseAndFlush();
}


static string? PromptForInput(string prompt, bool valueRequired)
{
    string? response;
    do
    {
        Console.WriteLine($"{prompt}:");
        response = Console.ReadLine();
        if (valueRequired && string.IsNullOrEmpty(response))
        {
            Console.WriteLine("You must provide a value");
        }
    } while (valueRequired && string.IsNullOrEmpty(response));

    return response;
}

static DateTime GetLastUploadTime()
{
    if (File.Exists("lastuploadtime.bin"))
    {
        return DateTime.Parse(
            File.ReadAllText("lastuploadtime.bin")).ToUniversalTime();
    }

    return DateTime.MinValue;
}

static void SaveLastUploadTime(DateTime uploadTime)
{
    File.WriteAllText("lastuploadtime.bin", uploadTime.ToString("u"));
}

void InitializeGraph(Settings settings)
{
    try
    {
        GraphHelper.Initialize(settings);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing Graph: {ex.Message}");
    }
}

async Task<ExternalConnection?> CreateConnectionAsync()
{
    var connectionId = PromptForInput(
        "Enter a unique ID for the new connection (3-32 characters)", true) ?? "ConnectionId";
    var connectionName = PromptForInput(
        "Enter a name for the new connection", true) ?? "ConnectionName";
    var connectionDescription = PromptForInput(
        "Enter a description for the new connection", false);

    try
    {
        // Create the connection
        var connection = await GraphHelper.CreateConnectionAsync(
            connectionId, connectionName, connectionDescription);
        Console.WriteLine($"New connection created - Name: {connection?.Name}, Id: {connection?.Id}");
        return connection;
    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error creating connection: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        return null;
    }
}


async Task<ExternalConnection?> SelectExistingConnectionAsync()
{
    Console.WriteLine("Getting existing connections...");
    try
    {
        var response = await GraphHelper.GetExistingConnectionsAsync();
        var connections = response?.Value ?? new List<ExternalConnection>();
        if (connections.Count <= 0)
        {
            Console.WriteLine("No connections exist. Please create a new connection");
            return null;
        }

        // Display connections
        Console.WriteLine("Choose one of the following connections:");
        var menuNumber = 1;
        foreach (var connection in connections)
        {
            Console.WriteLine($"{menuNumber++}. {connection.Name}");
        }

        ExternalConnection? selection = null;

        do
        {
            try
            {
                Console.Write("Selection: ");
                var choice = int.Parse(Console.ReadLine() ?? string.Empty);
                if (choice > 0 && choice <= connections.Count)
                {
                    selection = connections[choice - 1];
                }
                else
                {
                    Console.WriteLine("Invalid choice.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid choice.");
            }
        } while (selection == null);

        return selection;
    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error getting connections: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        return null;
    }
}

async Task DeleteCurrentConnectionAsync(ExternalConnection? connection)
{
    if (connection == null)
    {
        Console.WriteLine(
            "No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    try
    {
        await GraphHelper.DeleteConnectionAsync(connection.Id);
        Console.WriteLine($"{connection.Name} deleted successfully.");
    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error deleting connection: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
    }
}

async Task RegisterSchemaAsync(bool isUpdate = false)
{
    if (currentConnection == null)
    {
        Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    Console.WriteLine("Registering schema, this may take a moment...");

    try
    {
        // Create the schema
        var schema = new Schema
        {
            BaseType = "microsoft.graph.externalItem",
            Properties = new List<Property>
            {
                //new Property { Name = "documentId", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = true },
                new Property { Name = "latestTitle", Type = PropertyType.String, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false, Labels = new List<Label?>() { Label.Title }},
                new Property { Name = "latestVersion", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "documentClass", Type = PropertyType.String, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "type", Type = PropertyType.String, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "categories", Type = PropertyType.String, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "lastPublishedAt", Type = PropertyType.String, IsQueryable = false, IsSearchable = false, IsRetrievable = false, IsRefinable = false },
                new Property { Name = "documentUrl", Type = PropertyType.String, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false, Labels = new List<Label?>(){ Label.Url} }
            },
        };

        if (isUpdate)
        {
            await GraphHelper.RegisterSchemaAsync(currentConnection.Id, schema, true);
            Console.WriteLine("Schema registered successfully");
        }
        else 
        {
            await GraphHelper.RegisterSchemaAsync(currentConnection.Id, schema, false);
            Console.WriteLine("Schema registered successfully");
        }
    }
    catch (ServiceException serviceException)
    {
        Console.WriteLine($"Error registering schema: {serviceException.ResponseStatusCode} {serviceException.Message}");
    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error registering schema: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
    }
}

async Task GetSchemaAsync()
{
    if (currentConnection == null)
    {
        Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    try
    {
        var schema = await GraphHelper.GetSchemaAsync(currentConnection.Id);
        Console.WriteLine(JsonSerializer.Serialize(schema));

    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error getting schema: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
    }
}

async Task UpdateItemsFromDatabaseAsync(bool uploadModifiedOnly, string? tenantId)
{
    if (currentConnection == null)
    {
        Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    _ = tenantId ?? throw new ArgumentException("tenantId is null");

    List<ClausematchDocument>? documentsToUpload = null;
    List<ClausematchDocument>? documentsToDelete = null;

    var newUploadTime = DateTime.UtcNow;

    var documentsDb = new ClausematchDbContext();
    documentsDb.EnsureDatabase();

    if (uploadModifiedOnly)
    {
        var lastUploadTime = GetLastUploadTime();
        Console.WriteLine($"Uploading changes since last upload at {lastUploadTime.ToLocalTime()}");

        documentsToUpload = documentsDb.Documents
            .Where(p => EF.Property<DateTime>(p, "LastUpdated") > lastUploadTime)
            .ToList();

        documentsToDelete = documentsDb.Documents
            .IgnoreQueryFilters()
            .Where(p => EF.Property<bool>(p, "IsDeleted")
                && EF.Property<DateTime>(p, "LastUpdated") > lastUploadTime)
            .ToList();
    }
    else
    {
        documentsToUpload = documentsDb.Documents.ToList();

        documentsToDelete = documentsDb.Documents
            .IgnoreQueryFilters()
            .Where(p => EF.Property<bool>(p, "IsDeleted"))
            .ToList();
    }

    Console.WriteLine($"Processing {documentsToUpload.Count} add/updates, {documentsToDelete.Count} deletes.");
    var success = true;

    foreach (var document in documentsToUpload)
    {

        var newItem = new ExternalItem
        {
            Id = document.DocumentId.ToString(),
            Content = new ExternalItemContent
            {
                Type = ExternalItemContentType.Text,
                Value = document.LatestTitle
            },
            Acl = new List<Acl>
            {
                new Acl
                {
                    AccessType = AccessType.Grant,
                    Type = AclType.Everyone,
                    Value = tenantId,
                }
            },
            Properties = document.AsExternalItemProperties(),
        };
        newItem.Properties.AdditionalData.Remove("id");
        //var json = JsonSerializer.Serialize(newItem);
        //Console.WriteLine(json);
        try
        {
            Console.Write($"Uploading document: {document.LatestTitle} ({document.DocumentId})...");
            await GraphHelper.AddOrUpdateItemAsync(currentConnection.Id, newItem);
            Console.WriteLine("DONE");
        }
        catch (ODataError odataError)
        {
            success = false;
            Console.WriteLine("FAILED");
            Console.WriteLine($"Error: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        }
    }

    foreach (var document in documentsToDelete)
    {
        try
        {
            Console.Write($"Deleting document number {document.DocumentId}...");
            await GraphHelper.DeleteItemAsync(currentConnection.Id, document.DocumentId.ToString());
            Console.WriteLine("DONE");
        }
        catch (ODataError odataError)
        {
            success = false;
            Console.WriteLine("FAILED");
            Console.WriteLine($"Error: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        }
    }

    // If no errors, update our last upload time
    if (success)
    {
        SaveLastUploadTime(newUploadTime);
    }
  }

async Task DeleteAllDeletedItemsFromDatabaseAsync(string? tenantId)
{
    if (currentConnection == null)
    {
        Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    _ = tenantId ?? throw new ArgumentException("tenantId is null");

    using var documentsDb = new ClausematchDbContext();
    documentsDb.EnsureDatabase();

    var documentsToDelete = documentsDb.Documents
        .IgnoreQueryFilters()
        .Where(p => EF.Property<bool>(p, "IsDeleted"))
        .ToList();

    Console.WriteLine($"Found {documentsToDelete.Count} documents to delete from Graph.");

    var success = true;

    foreach (var document in documentsToDelete)
    {
        try
        {
            Console.Write($"Deleting document {document.DocumentId} from Graph...");
            await GraphHelper.DeleteItemAsync(currentConnection.Id, document.DocumentId.ToString());
            Console.WriteLine("DONE");
        }
        catch (ODataError odataError)
        {
            success = false;
            Console.WriteLine("FAILED");
            Console.WriteLine($"Error: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        }
    }

    if (success)
    {
        Console.WriteLine("All marked-deleted documents successfully removed from Graph.");
    }
    else
    {
        Console.WriteLine("Some deletions failed. Review logs for details.");
    }
}


