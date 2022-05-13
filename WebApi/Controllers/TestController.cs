/*using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers;

//[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly PoiContext _context;

    public TestController(PoiContext context)
    {
        _context = context;
    }

    [HttpGet("name")]
    public async Task<ActionResult> GetCategory(string name)
    {
        var names = name.Split(" > ").Reverse();
        var enumerable = names.ToList();
        name = enumerable[0];
        var e = await _context.Categories.Include(cat => cat.SubCategories)
            .FirstOrDefaultAsync(cat => cat.Name == name);
        if (e == null) return NotFound();
        return Ok(e.SubCategories);
    }
}
*/
