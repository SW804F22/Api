using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NuGet.Protocol;
using WebApi;
using WebApi.Controllers;
using WebApi.Models;
using WebApi.Services;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Test;

public class AuthenticationControllerTest : IClassFixture<TestDatabaseFixture>
{

    private readonly TestDatabaseFixture Fixture;

    private AuthenticationController Arrange(PoiContext context)
    {

        var auth = new AuthenticateService(new FakeSignInManager(context), new FakeUserManager(context));
        return new AuthenticationController(auth);
    }

    public AuthenticationControllerTest(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    [Fact]
    public async Task CanLogIn()
    {
        var controller = Arrange(Fixture.CreateContext());

        var username = "Test";
        var password = "TestPassword123";
        var result = await controller.Login(new Login(username, password));

        var res = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<UserDTO>(res.Value);
        Assert.Equal(username, value.UserName);
    }

    [Theory]
    [InlineData("wrongusername", "TestPassword123")]
    [InlineData("Test", "WrongPassword123")]
    [InlineData("WrongUsername", "WrongPassword123")]
    public async Task LoginFails(string username, string password)
    {
        var controller = Arrange(Fixture.CreateContext());

        var result = await controller.Login(new Login(username, password));
        Assert.IsNotType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CanCreateNewUser()
    {
        var context = Fixture.CreateContext();
        var controller = Arrange(context);

        context.Database.BeginTransaction();

        var reg = new Register("TestUser", "TestPassword123", DateTime.Today, 0);
        context.ChangeTracker.Clear();

        var result = await controller.Register(reg);
        Assert.IsType<CreatedResult>(result);
    }

}
