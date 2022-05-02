using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi;
using WebApi.Models;

namespace Test;

public class TestDatabaseFixture
{
    private const string ConnectionString = "Server=tcp:poirec.database.windows.net,1433;Database=test;Persist Security Info=False;User ID=poiadmin;Password=SW804F22srv!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    private IEnumerable<User> CreateUsers()
    {
        var result = new List<User>();
        var user1 = new User()
        { UserName = "Test", NormalizedUserName = "TEST", DateOfBirth = DateTime.Now, Gender = 0 };
        result.Add(user1);
        return result;
    }

    private IEnumerable<string> CreatePasswords()
    {
        return new[] { "TestPassword123" };
    }

    private IEnumerable<Category> CreateCategories()
    {
        var result = new List<Category>();
        var travel = new Category("Travel and Transportation", null);
        result.Add(travel);
        var lodging = new Category("Lodging", travel);
        result.Add(lodging);
        var hotel = new Category("Hotel", lodging);
        result.Add(hotel);
        return result;
    }

    private IEnumerable<Poi> CreatePois()
    {
        var result = new List<Poi>();
        return result;
    }

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
                    var hasher = new PasswordHasher<User>();
                    var _users = CreateUsers();
                    foreach (var p in Enumerable.Zip(_users, CreatePasswords()))
                    {
                        var hash = hasher.HashPassword(p.First, p.Second);
                        p.First.PasswordHash = hash;
                    }
                    context.AddRange(_users);
                    context.AddRange(CreateCategories());
                    context.AddRange(CreatePois());

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
        string[] entities = { "UserClaims", "Checkins", "CategoryPoi", "Users", "Categories", "Pois" };
        foreach (var e in entities)
        {
            context.Database.ExecuteSqlRaw($"delete from {e}");
        }

    }
}
