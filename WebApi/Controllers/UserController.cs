using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private PoiContext _context;
    private readonly UserManager<User> _userManager;

    public UserController(PoiContext context, UserManager<User> manager)
    {
        _context = context;
        _userManager = manager;
    }

    [HttpGet("id")]
    [SwaggerOperation(Summary = "Get user", Description = "Get user with id")]
    [SwaggerResponse(404, "USer not found")]
    [SwaggerResponse(200, "Success")]
    public async Task<ActionResult> GetUser([SwaggerParameter("Id of user to find")]string id)
    {
        var result = await _context.Users.FindAsync(id);
        if (result == null)
        {
            return NotFound("User not found");
        }
        return Ok(result);
    }

    [HttpPut("id")]
    public async Task<ActionResult> EditUser(string id, [FromBody] User user)
    {
        var result = await _context.Users.FindAsync(id);
        if (result == null)
        {
            return NotFound("User not found");
        }
        var changeResult = await _userManager.UpdateAsync(user);
        if (!changeResult.Succeeded)
        {
            return BadRequest("Update Failed");
        }
        
        
        
        throw new NotImplementedException();
    }

    [HttpDelete("id")]
    public Task<ActionResult> DeleteUser(string id)
    {
        throw new NotImplementedException();
    }
}
