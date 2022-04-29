using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;
using WebApi.Models.DTOs;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticateService _authenticationService;

    public AuthenticationController(AuthenticateService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [Route("Login")]
    [HttpPost]
    [SwaggerOperation(Summary = "Login user", Description = "Login using username and password")]
    [SwaggerResponse(200, "Login successful", typeof(UserDTO))]
    [SwaggerResponse(400, "Authentication failed")]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult> Login([FromBody][SwaggerRequestBody("Login information", Required = true)] Login l)
    {
        try
        {
            var result = await _authenticationService.Login(l.Username, l.Password);
            return result.Item1.Succeeded ? Ok(new UserDTO(result.Item2)) : ValidationProblem();
        }
        catch (InvalidOperationException)
        {
            return NotFound("User not found");
        }


    }

    [Route("Register")]
    [HttpPost]
    [SwaggerOperation(Summary = "Register user", Description = "Register a new user with username, password, date of birth and gender")]
    [SwaggerResponse(201, "Registration successful", typeof(UserDTO))]
    [SwaggerResponse(400, "Registration failed")]
    public async Task<ActionResult> Register([FromBody][SwaggerRequestBody("User information", Required = true)] Register info)
    {
        var user = new User
        {
            UserName = info.Username,
            DateOfBirth = info.DateOfBirth,
            Gender = info.Gender
        };
        var result = await _authenticationService.Register(user, info.Password);

        if (result.Succeeded) return Created("Registration complete!", new UserDTO(user));
        return BadRequest(result.Errors);
    }

    [Route("Password/{id}")]
    [HttpPost]
    [SwaggerOperation("Change password", "Change the password of a user with id")]
    [SwaggerResponse(200, "Password changed successful")]
    [SwaggerResponse(404, "User not found")]
    [SwaggerResponse(400, "Authentication failed")]
    public async Task<ActionResult> ChangePassword([SwaggerParameter("Id of user")][SwaggerSchemaExample("9b279878-74fd-46c4-8980-307f80375723")] string id, [FromBody][SwaggerRequestBody("Old and new password")] ChangePasswordDTO obj)
    {
        var user = await _authenticationService.FindUser(id);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var res = await _authenticationService.ChangePassword(user, obj.OldPassword, obj.NewPassword);
        if (res.Succeeded)
        {
            return Ok("Password changed successful");
        }
        return ValidationProblem(res.Errors.ToString());
    }
}
