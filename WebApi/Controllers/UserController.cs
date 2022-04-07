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
    private readonly PoiContext _context;

    public UserController(PoiContext context)
    {
        _context = context;
    }

    [HttpGet("id")]
    [SwaggerOperation(Summary = "Get user", Description = "Get user with id")]
    [SwaggerResponse(404, "User not found")]
    [SwaggerResponse(200, "Success", typeof(UserDTO))]
    public async Task<ActionResult> GetUser([SwaggerParameter("Id of user to find")] string id)
    {
        var result = await _context.Users.FindAsync(id);
        if (result == null)
        {
            return NotFound("User not found");
        }
        return Ok(new UserDTO(result));
    }

    [HttpPut("id")]
    [SwaggerOperation("Update user information", "Update the users username, gender of date of birth")]
    [SwaggerResponse(404, "User not found")]
    [SwaggerResponse(400, "Something went wrong")]
    [SwaggerResponse(200, "Update successful")]
    public async Task<ActionResult> EditUser([SwaggerParameter("Id of user to update")] string id, [FromBody][SwaggerRequestBody("User information to change")] UserDTO dto)
    {
        var result = await _context.Users.FindAsync(id);
        if (result == null)
        {
            return NotFound("User not found");
        }

        result.UserName = dto.UserName;
        result.Gender = dto.Gender;
        result.DateOfBirth = dto.DateOfBirth;

        var save = await _context.SaveChangesAsync();

        if (save == 1)
        {
            return Ok();
        }

        return BadRequest("Something went wrong");
    }

    [HttpDelete("id")]
    [SwaggerOperation("Delete user", "Delete user with id")]
    [SwaggerResponse(404, "User not found")]
    [SwaggerResponse(400, "Something went wrong")]
    [SwaggerResponse(200, "Delete successful")]
    public async Task<ActionResult> DeleteUser([SwaggerParameter("Id of user to delete")] string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }
        _context.Users.Remove(user);

        var delete = await _context.SaveChangesAsync();

        if (delete == 1)
        {
            return Ok("Delete successful");
        }

        return BadRequest("Something went wrong");
    }
}
