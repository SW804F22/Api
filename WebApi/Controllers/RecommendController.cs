using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RecommendController : ControllerBase
{
    [HttpPost]
    public Task<ActionResult> Recommend([FromBody] Recommend parameters)
    {
        throw new NotImplementedException();
    }
}
