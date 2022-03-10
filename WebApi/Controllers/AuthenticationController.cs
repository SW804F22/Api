using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Consumes("application/json")]
    public async Task<ActionResult> Login(Login l)
    {
        User user = await _userManager.FindByNameAsync(l.Username);
        //User user = await _context.Users.SingleAsync(u => u.UserName == username);
        var result = await _signInManager.PasswordSignInAsync(user, l.Password, true, false);
        return result.Succeeded ? Ok(user.Id) : ValidationProblem();
    }

    [Route("Register")]
    [HttpPost]
    public async Task<ActionResult> Register(string username, string password)
    {
        var user = new User
        {
            UserName = username
        };
        var result = await _userManager.CreateAsync(user, password);
        
        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest();

    }
}