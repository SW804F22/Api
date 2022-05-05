using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Controllers;
using WebApi.Models;
using WebApi.Models.DTOs;
using WebApi.Services;
using Xunit;
namespace Test;

public class PoiControllerTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture Fixture;

    public PoiControllerTest(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    private async Task CreatePois(PoiController controller)
    {
        var poi0 = new PoiDTO()
        {
            Title = "Absalon Hotel",
            Latitude = 55.671565,
            Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk",
            Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK",
            PriceStep = Price.Free,
            Categories = new List<string>() { "Hotel" }
        };
        var poi1 = new PoiDTO()
        {
            Title = "Test poi",
            Latitude = 55.671565,
            Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk",
            Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK",
            PriceStep = Price.Free,
            Categories = new List<string>() { "Hotel" }
        };
        var poi2 = new PoiDTO()
        {
            Title = "Café Europa",
            Latitude = 55.678662,
            Longitude = 12.579335,
            Description = "",
            Website = "http://europa1989.dk/",
            Address = "Amagertorv 1 (Højbro Plads), 1160 København, DK",
            PriceStep = Price.Cheap,
            Categories = new List<string>() { "Restaurant", "Coffee Shop" }
        };

        await controller.CreatePoi(poi0);
        await controller.CreatePoi(poi1);
        await controller.CreatePoi(poi2);
    }

    [Fact]
    [Group("Create Poi")]
    public async Task CreatePoiSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var dto = new PoiDTO()
        {
            Title = "Absalon Hotel",
            Latitude = 55.671565,
            Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk",
            Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK",
            PriceStep = Price.Free,
            Categories = new List<string>() { "Hotel" }
        };

        await context.Database.BeginTransactionAsync();

        var result = await controller.CreatePoi(dto);
        var res = Assert.IsType<CreatedResult>(result);
        var poi = Assert.IsType<PoiDTO>(res.Value);

        Assert.Equal(dto.Title, poi.Title);
        Assert.NotNull(context.Pois.FirstOrDefault(p => p.Title == dto.Title));
        Assert.Equal(3, context.Pois.Include(p => p.Categories).First(p => p.Title == dto.Title).Categories.Count);

        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Create Poi")]
    public async Task CreateFailsWhenCategoryNotFound()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var dto = new PoiDTO()
        {
            Title = "Absalon Hotel",
            Latitude = 55.671565,
            Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk",
            Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK",
            PriceStep = Price.Free,
            Categories = new List<string>() { "Unvalid category" }
        };

        await context.Database.BeginTransactionAsync();

        var result = await controller.CreatePoi(dto);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    public static IEnumerable<object[]> Data()
    {
        yield return new object[] { null! };
        yield return new object[] { new List<string>() };
    }

    [Theory]
    [Group("Create Poi")]
    [MemberData(nameof(Data))]
    public async Task CreateFails400WhenNoCategory(List<string>? categories)
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var dto = new PoiDTO()
        {
            Title = "Absalon Hotel",
            Latitude = 55.671565,
            Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk",
            Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK",
            PriceStep = Price.Free,
            Categories = categories
        };

        await context.Database.BeginTransactionAsync();

        var result = await controller.CreatePoi(dto);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Group("Get Poi")]
    public async Task GetPoiSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var poi = context.Pois.First(p => p.Title == "Absalon Hotel");

        var result = await controller.GetPoi(poi.UUID.Value);

        var res = Assert.IsType<OkObjectResult>(result);
        var poires = Assert.IsType<PoiDTO>(res.Value);
        Assert.Equal(poi.UUID, poires.id);
        Assert.Equal(poi.Title, poires.Title);

        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Get Poi")]
    public async Task GetFailsWhenPoiNotExist()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var id = Guid.NewGuid();

        var result = await controller.GetPoi(id);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    [Group("Edit Poi")]
    public async Task EditSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);
        var poi = context.Pois.First(p => p.Title == "Absalon Hotel");

        var dto = new PoiDTO()
        {
            id = poi.UUID,
            Title = "Hotel Absalon",
            Latitude = 55.7,
            Longitude = 12.6,
            Description = "New description",
            Website = "test web",
            Address = "test address",
            PriceStep = Price.Expensive,
            Categories = null
        };

        var result = await controller.EditPoi(dto.id.Value, dto);
        Assert.IsType<OkObjectResult>(result);

        Assert.Null(context.Pois.FirstOrDefault(p => p.Title == "Absalon Hotel"));
        Assert.NotNull(context.Pois.FirstOrDefault(p => p.Title == "Hotel Absalon"));
        Assert.NotNull(context.Pois.FirstOrDefault(p => Math.Abs(p.Latitude - 55.7) < 0.01));
        Assert.NotNull(context.Pois.FirstOrDefault(p => Math.Abs(p.Longitude - 12.6) < 0.01));
        Assert.NotNull(context.Pois.FirstOrDefault(p => p.Description == "New description"));
        Assert.NotNull(context.Pois.FirstOrDefault(p => p.Website == "test web"));
        Assert.NotNull(context.Pois.FirstOrDefault(p => p.Address == "test address"));
        Assert.NotNull(context.Pois.FirstOrDefault(p => p.PriceStep == Price.Expensive));

        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Edit Poi")]
    public async Task EditFailsWhenWrongId()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var dto = new PoiDTO()
        {
            id = Guid.NewGuid(),
            Title = "Hotel Absalon",
            Latitude = 55.7,
            Longitude = 12.6,
            Description = "New description",
            Website = "test web",
            Address = "test address",
            PriceStep = Price.Expensive,
            Categories = null
        };

        var result = await controller.EditPoi(dto.id.Value, dto);
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Delete Poi")]
    public async Task DeleteSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var poi = context.Pois.First(p => p.Title == "Absalon Hotel");

        var result = await controller.DeletePoi(poi.UUID.Value);
        Assert.IsType<OkObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Delete Poi")]
    public async Task DeleteFailsWhenWrongId()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.DeletePoi(Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Search Category")]
    public void FindCategories()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var result = controller.SearchCategory("bar");
        var res = Assert.IsType<OkObjectResult>(result);
        var cats = Assert.IsType<string[]>(res.Value);
        Assert.NotEmpty(cats);
        Assert.Equal(context.Categories.Count(), cats.Length);
        Assert.Matches(new Regex("Bar"), cats.First());
    }

    [Fact]
    [Group("Search Category")]
    public void AllCategoriesWhenQueryNull()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var result = controller.SearchCategory(null);
        var res = Assert.IsType<OkObjectResult>(result);
        var cats = Assert.IsType<string[]>(res.Value);
        Assert.NotEmpty(cats);
        Assert.Equal(context.Categories.Count(), cats.Length);

    }

    [Theory]
    [Group("Search Category")]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    public void SearchCategoryLimitWorks(int limit)
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var result = controller.SearchCategory("bar", limit);
        var res = Assert.IsType<OkObjectResult>(result);
        var cats = Assert.IsType<string[]>(res.Value);
        Assert.Equal(limit, cats.Length);
    }

    [Theory]
    [Group("Search Category")]
    [InlineData("")]
    [InlineData("Test")]
    [InlineData("Hotel")]
    [InlineData("Bar")]
    [InlineData("Gibberish")]
    [InlineData("jffsgldf")]
    public void CategorySearchAnyQueryYieldsResults(string query)
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var result = controller.SearchCategory(null);
        var res = Assert.IsType<OkObjectResult>(result);
        var cats = Assert.IsType<string[]>(res.Value);
        Assert.NotEmpty(cats);
        Assert.Equal(context.Categories.Count(), cats.Length);
    }

    [Theory]
    [Group("Search Poi Name")]
    [InlineData("Abs")]
    [InlineData("Absalon")]
    [InlineData("absalon")]
    [InlineData("Absalon Hotel")]
    [InlineData("bbsalom")]
    public async Task IncrementalSearchGivesSameResult(string query)
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = controller.SearchName(query);
        var res = Assert.IsType<OkObjectResult>(result);
        var pois = Assert.IsType<string[]>(res.Value);
        Assert.Equal(context.Pois.Count(), pois.Length);
        Assert.Equal("Absalon Hotel", pois.First());
    }

    [Fact]
    [Group("Search Poi Name")]
    public async Task SearchGibberishStillResult()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = controller.SearchName("This is a test that doesn't match any poi");
        var res = Assert.IsType<OkObjectResult>(result);
        var pois = Assert.IsType<string[]>(res.Value);
        Assert.Equal(context.Pois.Count(), pois.Length);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Poi Search")]
    public async Task SearchNameSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search("Hotel", Enumerable.Empty<string>(), Enumerable.Empty<string>(), null, null, null, Enumerable.Empty<Price>());
        var res = Assert.IsType<OkObjectResult>(result);
        var pois = Assert.IsType<PoiDTO[]>(res.Value);
        Assert.Equal(1, pois.Length);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Poi Search")]
    public async Task SearchCategorySuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search(null, new[] { "Hotel" }, Enumerable.Empty<string>(), null, null, null, Enumerable.Empty<Price>());
        var res = Assert.IsType<OkObjectResult>(result);
        var pois = Assert.IsType<PoiDTO[]>(res.Value);
        Assert.Equal(2, pois.Length);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Poi Search")]
    public async Task SearchNotCategorySuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search(null, Enumerable.Empty<string>(), new[] { "Hotel" }, null, null, null, Enumerable.Empty<Price>());
        var res = Assert.IsType<OkObjectResult>(result);
        var pois = Assert.IsType<PoiDTO[]>(res.Value);
        Assert.Equal(1, pois.Length);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Poi Search")]
    public async Task SearchPositionSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search(null, Enumerable.Empty<string>(), Enumerable.Empty<string>(), 55.67, 12.57, 1, Enumerable.Empty<Price>());
        var res = Assert.IsType<OkObjectResult>(result);
        var pois = Assert.IsType<PoiDTO[]>(res.Value);
        Assert.Equal(3, pois.Length);

        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Poi Search")]
    public async Task SearchPriceSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search(null, Enumerable.Empty<string>(), Enumerable.Empty<string>(), null, null, null, new[]
            {
                Price.Free
            });
        var res = Assert.IsType<OkObjectResult>(result);
        var pois = Assert.IsType<PoiDTO[]>(res.Value);
        Assert.Equal(2, pois.Length);

        context.ChangeTracker.Clear();
    }

    [Theory]
    [Group("Search Poi")]
    [InlineData(null, 12, 0.1)]
    [InlineData(55, null, 0.1)]
    [InlineData(55, 12, null)]
    public async Task SearchFailsWhenAnyLocationAttributeMissing(double? lat, double? lon, double? dist)
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search(null, Enumerable.Empty<string>(), Enumerable.Empty<string>(), lat, lon, dist, Enumerable.Empty<Price>());
        Assert.IsType<BadRequestObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Search Poi")]
    public async Task SearchNotFoundWhenWrongNotCategories()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search(null, Enumerable.Empty<string>(), new[] { "This is not a category" }, null, null, null, Enumerable.Empty<Price>());
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Search Poi")]
    public async Task SearchNotFoundWhenWrongCategories()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search(null, new[] { "This is not a category" }, Enumerable.Empty<string>(), null, null, null, Enumerable.Empty<Price>());
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Search Poi")]
    public async Task SeachNotFoundWhenNoMatch()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));
        await context.Database.BeginTransactionAsync();
        await CreatePois(controller);

        var result = await controller.Search("This is a test and should not match", Enumerable.Empty<string>(), Enumerable.Empty<string>(), null, null, null, Enumerable.Empty<Price>());
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }

}
