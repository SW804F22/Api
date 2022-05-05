using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApi;
using WebApi.Controllers;
using WebApi.Models;
using WebApi.Models.DTOs;
using WebApi.Services;
using Xunit;

namespace Test;

public class UserControllerTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture Fixture;

    public UserControllerTest(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    private async Task CreatePois(PoiContext context)
    {
        var controller = new PoiController(context, new SearchService(context));
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
    [Group("Get User")]
    public async Task GetUserSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);

        var user = context.Users.First(u => u.UserName == "Test");
        var result = await controller.GetUser(user.Id);
        var res = Assert.IsType<OkObjectResult>(result);
        var resuser = Assert.IsType<UserDTO>(res.Value);
        Assert.Equal(user.UserName, resuser.UserName);
    }

    [Fact]
    [Group("Get User")]
    public async Task GetUserNotFoundWithRandomId()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);

        var result = await controller.GetUser(Guid.NewGuid().ToString());
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    [Group("Edit User")]
    public async Task EditUserSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);
        var user = context.Users.First(u => u.UserName == "Test");

        var dto = new UserDTO(user)
        {
            UserName = "TestUser123",
            DateOfBirth = DateTime.MinValue,
            Gender = Gender.Female
        };
        await context.Database.BeginTransactionAsync();
        var result = await controller.EditUser(user.Id, dto);
        Assert.IsType<OkObjectResult>(result);
        Assert.Null(context.Users.FirstOrDefault(u => u.UserName == "Test"));
        Assert.NotNull(context.Users.FirstOrDefault(u => u.UserName == "TestUser123"));
        user = context.Users.First(u => u.UserName == "TestUser123");
        Assert.Equal(Gender.Female, user.Gender);
        Assert.Equal(DateTime.MinValue, user.DateOfBirth);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Edit User")]
    public async Task EditUserNotFoundWrongId()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);
        var user = context.Users.First(u => u.UserName == "Test");

        var dto = new UserDTO(user)
        {
            UserName = "TestUser123",
            DateOfBirth = DateTime.MinValue,
            Gender = Gender.Female
        };

        var result = await controller.EditUser(Guid.NewGuid().ToString(), dto);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    [Group("Delete User")]
    public async Task DeleteUserSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);
        var user = context.Users.First(u => u.UserName == "Test");

        await context.Database.BeginTransactionAsync();

        var result = await controller.DeleteUser(user.Id);
        Assert.IsType<OkObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Delete User")]
    public async Task DeleteUserNotFoundWrongId()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);

        var result = await controller.DeleteUser(Guid.NewGuid().ToString());
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    [Group("Get Checkins")]
    public async Task GetCheckinsSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);
        await context.Database.BeginTransactionAsync();
        await CreatePois(context);

        var user = context.Users.First();
        var poi = context.Pois.First();
        await controller.Visit(user.Id, poi.UUID.Value);

        var result = await controller.GetCheckins(user.Id);
        var res = Assert.IsType<OkObjectResult>(result);
        var checkins = Assert.IsType<Checkin[]>(res.Value);
        Assert.Equal(1, checkins.Length);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Get Checkins")]
    public async Task GetCheckinsNotFoundWrongId()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);

        var result = await controller.GetCheckins(Guid.NewGuid().ToString());
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    [Group("Visit")]
    public async Task CreateCheckinSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);
        
        await context.Database.BeginTransactionAsync();
        await CreatePois(context);

        var user = context.Users.First();
        var poi = context.Pois.First();
        var result = await controller.Visit(user.Id, poi.UUID.Value);
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, context.Checkins.Count());
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Visit")]
    public async Task CreateCheckinNotFoundInvalidUser()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);
        await context.Database.BeginTransactionAsync();
        await CreatePois(context);
        var user = context.Users.First();
        var poi = context.Pois.First();
        var result = await controller.Visit(Guid.NewGuid().ToString(), poi.UUID.Value);
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }
    
    [Fact]
    [Group("Visit")]
    public async Task CreateCheckinNotFoundInvalidPoi()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);
        await context.Database.BeginTransactionAsync();
        await CreatePois(context);
        var user = context.Users.First();
        var poi = context.Pois.First();
        var result = await controller.Visit(user.Id, Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(result);
        context.ChangeTracker.Clear();
    }

    [Fact]
    [Group("Unvisit")]
    public async Task RemoveCheckinSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);
        await context.Database.BeginTransactionAsync();
        await CreatePois(context);
        var user = context.Users.First();
        var poi = context.Pois.First();
        await controller.Visit(user.Id, poi.UUID.Value);

        var checkin = context.Checkins.First();
        var result = await controller.UnVisit(checkin.UUID);
        Assert.IsType<OkObjectResult>(result);
        context.ChangeTracker.Clear();
    }
    
    [Fact]
    [Group("Unvisit")]
    public async Task RemoveCheckinNotFoundWrongId()
    {
        var context = Fixture.CreateContext();
        var controller = new UserController(context);

        var result = await controller.UnVisit(Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(result);

    }
    
    
}
