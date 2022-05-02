using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Title ="Absalon Hotel", Latitude = 55.671565,Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk", Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK", PriceStep = Price.Free,
            Categories = new List<string>(){"Hotel"}
        };

        await controller.CreatePoi(poi0);
    }

    [Fact]
    [Group("Create Poi")]
    public async Task CreatePoiSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var dto = new PoiDTO()
        {
            Title ="Absalon Hotel", Latitude = 55.671565,Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk", Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK", PriceStep = Price.Free,
            Categories = new List<string>(){"Hotel"}
        };

        await context.Database.BeginTransactionAsync();
        
        var result = await controller.CreatePoi(dto);
        var res = Assert.IsType<CreatedResult>(result);
        var poi = Assert.IsType<PoiDTO>(res.Value);
        
        Assert.Equal(dto.Title, poi.Title);
        Assert.NotNull(context.Pois.FirstOrDefault(p => p.Title == dto.Title));
        Assert.Equal(3, context.Pois.Include(p => p.Categories).First(p=> p.Title == dto.Title).Categories.Count);
        
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
            Title ="Absalon Hotel", Latitude = 55.671565,Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk", Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK", PriceStep = Price.Free,
            Categories = new List<string>(){"Unvalid category"}
        };

        await context.Database.BeginTransactionAsync();
        
        var result = await controller.CreatePoi(dto);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    [Group("Create Poi")]
    public async Task CreateFails400WhenNoCategory()
    {
        var context = Fixture.CreateContext();
        var controller = new PoiController(context, new SearchService(context));

        var dto = new PoiDTO()
        {
            Title ="Absalon Hotel", Latitude = 55.671565,Longitude = 12.561658,
            Description = "Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops",
            Website = "http://www.absalon-hotel.dk", Address = "Helgolandsgade 15 (Istedgade), 1653 København, DK", PriceStep = Price.Free,
            Categories = new List<string>()
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
    
    
}
