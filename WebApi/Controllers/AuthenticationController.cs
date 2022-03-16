using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private SignInManager<User> _signInManager;
    private PoiContext _context;
    private UserManager<User> _userManager; 

    public AuthenticationController(PoiContext context, SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _context = context;
        _userManager = userManager;
    }
    
    [Route("Login")]
    [HttpPost]
    [SwaggerOperation(Summary = "Login user", Description = "Login using username and password")]
    [SwaggerResponse(200, "Login successful", typeof(Guid))]
    [SwaggerResponse(400,"Authentication failed")]
    public async Task<ActionResult> Login([FromBody, SwaggerRequestBody("Login information", Required = true)]Login l)
    {
        User user = await _userManager.FindByNameAsync(l.Username);
        var result = await _signInManager.PasswordSignInAsync(user, l.Password, true, false);
        return result.Succeeded ? Ok(user.Id) : ValidationProblem();
    }

    [Route("Register")]
    [HttpPost]
    public async Task<ActionResult> Register([FromBody] Register info)
    {
        var user = new User
        {
            UserName = info.Username,
            DateOfBirth = info.DateOfBirth,
            Gender = info.Gender
        };
        var result = await _userManager.CreateAsync(user, info.Password);

        if (result.Succeeded)
        {
            return Ok(user);
        }
        return BadRequest(result.Errors);

    }
    
}