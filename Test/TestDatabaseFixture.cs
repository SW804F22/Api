using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
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
        { UserName = "Test", NormalizedUserName = "TEST", DateOfBirth = DateTime.Now, Gender = Gender.Unspecified };
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
        var dinning = new Category("Dining and Drinking", null);
        result.Add(dinning);
        var bar = new Category("Bar", dinning);
        result.Add(bar);
        var beer = new Category("Beer Bar", bar);
        result.Add(beer);
        var cocktail = new Category("Cocktail Bar", bar);
        result.Add(cocktail);
        var sportsbar = new Category("Sports Bar", bar);
        result.Add(sportsbar);
        var restaurant = new Category("Restaurant", dinning);
        result.Add(restaurant);
        var cafe = new Category("Café", dinning);
        result.Add(cafe);
        var coffe = new Category("Coffee Shop", cafe);
        result.Add(coffe);
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
                    context.Database.SetCommandTimeout(60);
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
