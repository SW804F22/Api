using System;
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
    [Group("Login")]
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
    [Group("Login")]
    public async Task LoginFails(string username, string password)
    {
        var controller = Arrange(Fixture.CreateContext());

        var result = await controller.Login(new Login(username, password));
        Assert.IsNotType<OkObjectResult>(result);
    }

    [Fact]
    [Group("Register")]
    public async Task CanCreateNewUser()
    {
        var context = Fixture.CreateContext();
        var controller = Arrange(context);

        context.Database.BeginTransaction();

        var reg = new Register("TestUser", "TestPassword123", DateTime.Today, 0);
        context.ChangeTracker.Clear();

        var result = await controller.Register(reg);
        var res = Assert.IsType<CreatedResult>(result);
        var user = Assert.IsType<UserDTO>(res.Value);
        Assert.NotNull(context.Users.First(u => u.Id == user.Id));
    }

    [Theory]
    [InlineData("testpassword123")]
    [InlineData("TESTPASSWORD123")]
    [InlineData("TestPassword")]
    [InlineData("Test123")]
    [Group("Register")]
    public async Task RegisterFailsWhenPasswordNotValid(string password)
    {
        var context = Fixture.CreateContext();
        var controller = Arrange(context);

        var reg = new Register("TestUser", password, DateTime.Today, 0);

        var result = await controller.Register(reg);
        Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    [Group("Change password")]
    public async Task ChangePasswordSuccess()
    {
        var context = Fixture.CreateContext();
        var controller = Arrange(context);

        var user = context.Users.First(u => u.UserName == "Test");
        var pwd = new ChangePasswordDTO() { OldPassword = "TestPassword123", NewPassword = "TestPassword321" };
        await context.Database.BeginTransactionAsync();
        var result = await controller.ChangePassword(user.Id, pwd);

        var res = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, res.StatusCode);
        Assert.Equal("Password changed successful", res.Value);

        var oldLogin = await controller.Login(new Login("Test", "TestPassword123"));
        Assert.IsNotType<OkObjectResult>(oldLogin);

        var newLogin = await controller.Login(new Login("Test", "TestPassword321"));
        Assert.IsType<OkObjectResult>(newLogin);
        context.ChangeTracker.Clear();
    }
    [Fact]
    [Group("Change password")]
    public async Task ChangePasswordFailsWhenWrongUser()
    {
        var context = Fixture.CreateContext();
        var controller = Arrange(context);

        var userid = Guid.NewGuid();
        var pwd = new ChangePasswordDTO() { OldPassword = "Test", NewPassword = "Teeeest" };

        var result = await controller.ChangePassword(userid.ToString(), pwd);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    [Group("Change password")]
    public async Task ChangePasswordFailsWithWrongPassword()
    {
        var context = Fixture.CreateContext();
        var controller = Arrange(context);
        
        var user = context.Users.First(u => u.UserName == "Test");
        var pwd = new ChangePasswordDTO() { OldPassword = "VeryWrongPassword", NewPassword = "TestPassword321" };
        
        var result = await controller.ChangePassword(user.Id, pwd);
        Assert.IsNotType<OkObjectResult>(result);

    }
}
