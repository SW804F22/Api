using Microsoft.EntityFrameworkCore;
using WebApi;
using WebApi.Models;

namespace Test;

public class TestDatabaseFixture
{
    private const string ConnectionString = "Server=tcp:poirec.database.windows.net,1433;Database=test;Persist Security Info=False;User ID=poiadmin;Password=SW804F22srv!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureCreated();
                    Clear(context);
                    context.AddRange(
                        new User() {UserName = "test"});
                    context.SaveChanges();
                }

                _databaseInitialized = true;
            }
        }
    }

    public PoiContext CreateContext()
        => new PoiContext(
            new DbContextOptionsBuilder<PoiContext>()
                .UseSqlServer(ConnectionString)
                .Options);

    private void Clear(PoiContext context)
    {
        string[] entities = {"UserClaims", "Checkins", "CategoryPoi", "Users", "Categories", "Pois"};
        foreach (var e in entities)
        {
            context.Database.ExecuteSqlRaw($"delete from {e}");   
        }
        
    }
}
