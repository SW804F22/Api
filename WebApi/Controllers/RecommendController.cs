using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using System.Net.Http;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RecommendController : ControllerBase
{
    public RecommendController()
    {
        _client = new HttpClient();
    } 
    
    private HttpClient _client;
    [HttpPost]
    public Task<ActionResult> Recommend([FromBody] Recommend parameters)
    {
        
        throw new NotImplementedException();
    }
}
