using System.Text.Json;
using ClauseMatchGraphConnector.ClausematchApiClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ClauseMatchGraphConnector.Data;

public class ClausematchDbContext : DbContext
{
        public DbSet<ClausematchDocument> Documents => Set<ClausematchDocument>();

    public void EnsureDatabase()
    {
        var settings = Settings.LoadSettings();
        if (Database.EnsureCreated() || !Documents.Any())
        {
            // var documents = ClausematchDocumentGenerator.GenerateDocuments(50);
            var documents = ApiClientOrchestrator.GetClauseMatchDocumentsAsync(settings).Result;
            Documents.AddRange(documents);
            SaveChanges();
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=documents.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // EF Core can't store lists, so add a converter for the Appliances
        // property to serialize as a JSON string on save to DB
        ////modelBuilder.Entity<ClausematchDocument>()
        ////    .Property(ap => ap.Appliances)
        ////    .HasConversion(
        ////        v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
        ////        v => JsonSerializer.Deserialize<List<string>>(v, JsonSerializerOptions.Default)
        ////    );
        modelBuilder.Entity<ClausematchDocument>()
            .Ignore(d => d.LatestCategories);

        // Add LastUpdated and IsDeleted shadow properties
        modelBuilder.Entity<ClausematchDocument>()
            .Property<DateTime>("LastUpdated")
            .HasDefaultValueSql("datetime()")
            .ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<ClausematchDocument>()
            .Property<bool>("IsDeleted")
            .IsRequired()
            .HasDefaultValue(false);

        // Exclude any soft-deleted items (IsDeleted = 1) from
        // the default query sets
        modelBuilder.Entity<ClausematchDocument>()
            .HasQueryFilter(a => !EF.Property<bool>(a, "IsDeleted"));
    }

    public override int SaveChanges()
    {
        // Prevent deletes of data, instead mark the item as deleted
        // by setting IsDeleted = true.
        foreach(var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted))
        {
            if (entry.Entity.GetType() == typeof(ClausematchDocument))
            {
                SoftDelete(entry);
            }
        }

        return base.SaveChanges();
    }

    private void SoftDelete(EntityEntry entry)
    {
        var id = new SqliteParameter("@id",
            entry.OriginalValues["id"]);

        Database.ExecuteSqlRaw(
            "UPDATE documents SET IsDeleted = 1 WHERE Id = @id",
            id);

        entry.State = EntityState.Detached;
    }
}