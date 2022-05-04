using System;
using System.Collections.Generic;
using System.Linq;
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

        await controller.CreatePoi(poi0);
        await controller.CreatePoi(poi1);
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
    
    public static IEnumerable<object[]> Data(){
        yield return new object[] { null};
        yield return new object[] { new List<string>()};
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
        Assert.All(cats, c=> c.Contains("bar"));
    } 


}
