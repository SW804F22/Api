using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private PoiContext _context;

    public UserController(PoiContext context)
    {
        _context = context;
    }

    [HttpGet("id")]
    public Task<ActionResult> GetUser(string id)
    {
        throw new NotImplementedException();
    }

    [HttpPut("id")]
    public Task<ActionResult> EditUser(string id)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("id")]
    public Task<ActionResult> DeleteUser(string id)
    {
        throw new NotImplementedException();
    }
}
