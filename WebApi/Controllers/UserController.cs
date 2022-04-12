using System.Security.Cryptography.X509Certificates;
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

        if (dto.UserName != result.UserName)
        {
            result.UserName = dto.UserName;
            result.NormalizedUserName = dto.UserName.Normalize();
        }

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

    [HttpGet]
    [Route("visit/{id}")]
    [SwaggerOperation("Get check in's", "Get list of PoI's the user have visited")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<Checkin>))]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult> GetCheckins([SwaggerParameter("Id of user")] string id)
    {
        try
        {
            var user = await _context.Users.Include(u => u.Checkins).ThenInclude(c => c.Point).FirstAsync(u => u.Id == id);
            return Ok(user.Checkins);
        }
        catch (InvalidOperationException)
        {
            return NotFound("User not found");
        }
    }

    [HttpPost]
    [Route("visit/{userId}/{poiId}")]
    [SwaggerOperation("Register checkin", "Register checkin by user at a point of interest")]
    [SwaggerResponse(200, "Checkin added")]
    [SwaggerResponse(404, "Not found")]
    [SwaggerResponse(400, "Something went wrong")]
    public async Task<ActionResult> Visit([SwaggerParameter("User that checks in")] string userId, [SwaggerParameter("Point of interest checking in to")] Guid poiId)
    {
        try
        {
            var user = await _context.Users.Include(u => u.Checkins).FirstAsync(u => u.Id == userId);
            var poi = await _context.Pois.FindAsync(poiId);
            if (poi == null)
            {
                return NotFound("PoI not found");
            }
            var checkin = new Checkin(poi, DateTime.Now);
            var add = await _context.Checkins.AddAsync(checkin);
            user.Checkins.Add(add.Entity);
            var res = await _context.SaveChangesAsync();
            if (res > 0)
            {
                return Ok("Check in added");
            }
            return BadRequest("Something went wrong");
        }
        catch (InvalidOperationException)
        {
            return NotFound("User not found");
        }
    }

    [Route("visit/{id}")]
    [HttpDelete]
    [SwaggerOperation("Delete a checkin", "Remove a checkin at a location the user no longer likes")]
    [SwaggerResponse(200, "Checkin removed successfully")]
    [SwaggerResponse(404, "Checkin not found")]
    [SwaggerResponse(400, "Something went wrong")]
    public async Task<ActionResult> UnVisit([SwaggerParameter("Id of check in")] Guid id)
    {
        var checkin = await _context.Checkins.FindAsync(id);
        if (checkin == null)
        {
            return NotFound("Checkin not found");
        }
        _context.Checkins.Remove(checkin);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            return Ok("Checking removed successfully");
        }

        return BadRequest("Something went wrong");
    }
}
