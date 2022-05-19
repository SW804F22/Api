using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;
using WebApi.Models;
using WebApi.Models.DTOs;
using WebApi.Services;
using Xunit;

namespace Test;

public class RecommendControllerTest : IClassFixture<TestDatabaseFixture>
{

    private readonly TestDatabaseFixture Fixture;

    public RecommendControllerTest(TestDatabaseFixture fixture)
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
    [Group("Recommend")]
    public async Task RecommendSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new RecommendController(new MockRecommenderService(), new SearchService(context), context);
        await context.Database.BeginTransactionAsync();
        await CreatePois(new PoiController(context, new SearchService(context)));
        var user = context.Users.First();
        var rec = new Recommend()
        {
            Latitude = 55.6,
            Longitude = 12.6,
            Range = 2,
            UserID = user.Id
        };
        var result = await controller.Recommend(rec);
        var res = Assert.IsType<OkObjectResult>(result);
        var pois = Assert.IsType<PoiDTO[]>(res.Value);
        Assert.Equal(3, pois.Length);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Recommend")]
    public async Task RecommendFailsWhenUSerNotFound()
    {
        var context = Fixture.CreateContext();
        var controller = new RecommendController(new MockRecommenderService(), new SearchService(context), context);
        await context.Database.BeginTransactionAsync();
        await CreatePois(new PoiController(context, new SearchService(context)));
        var rec = new Recommend()
        {
            Latitude = 55.6,
            Longitude = 12.6,
            Range = 2,
            UserID = Guid.NewGuid().ToString()
        };
        var result = await controller.Recommend(rec);
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Recommend")]
    public async Task RecommendFailsWhenPoisNotFound()
    {
        var context = Fixture.CreateContext();
        var controller = new RecommendController(new MockRecommenderService(), new SearchService(context), context);
        await context.Database.BeginTransactionAsync();
        await CreatePois(new PoiController(context, new SearchService(context)));
        var user = context.Users.First();
        var rec = new Recommend()
        {
            Latitude = 0,
            Longitude = 0,
            Range = 0,
            UserID = user.Id
        };
        var result = await controller.Recommend(rec);
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }




}
