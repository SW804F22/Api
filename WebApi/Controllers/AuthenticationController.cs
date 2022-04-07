using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;
using WebApi.Models.DTOs;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthenticationController(SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [Route("Login")]
    [HttpPost]
    [SwaggerOperation(Summary = "Login user", Description = "Login using username and password")]
    [SwaggerResponse(200, "Login successful", typeof(UserDTO))]
    [SwaggerResponse(400, "Authentication failed")]
    public async Task<ActionResult> Login([FromBody][SwaggerRequestBody("Login information", Required = true)] Login l)
    {
        var user = await _userManager.FindByNameAsync(l.Username);
        var result = await _signInManager.PasswordSignInAsync(user, l.Password, true, false);
        return result.Succeeded ? Ok(new UserDTO(user)) : ValidationProblem();
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
        var result = await _userManager.CreateAsync(user, info.Password);

        if (result.Succeeded) return Created("Registration complete!", new UserDTO(user));
        return BadRequest(result.Errors);
    }

    [Route("Password/{id}")]
    [HttpPost]
    [SwaggerOperation("Change password", "Change the password of a user with id")]
    [SwaggerResponse(200, "Password changed successful")]
    [SwaggerResponse(404, "User not found")]
    [SwaggerResponse(400, "Authentication failed")]
    public async Task<ActionResult> ChangePassword([SwaggerParameter("Id of user")] string id, [FromBody][SwaggerRequestBody("Old and new password")] ChangePasswordDTO obj)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }
        var res = await _userManager.ChangePasswordAsync(user, obj.OldPassword, obj.NewPassword);
        if (res.Succeeded)
        {
            return Ok("Password changed successful");
        }
        return ValidationProblem(res.Errors.ToString());
    }
}
