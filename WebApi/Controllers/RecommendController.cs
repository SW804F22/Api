using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RecommendController : ControllerBase
{
    public RecommendController(RecommenderService recommender, SearchService search)
    {
        _recommender = recommender;
        _search = search;
    }
    private readonly RecommenderService _recommender;
    private readonly SearchService _search;
    [HttpPost]
    public async Task<ActionResult> Recommend([FromBody] Recommend parameters)
    {
        var pois = _search.Range(parameters.Latitude, parameters.Longitude, parameters.Range);
        if (!pois.Any())
        {
            return NotFound();
        }
        var res = await _recommender.PostRecommendation(parameters.UserID, pois);
        return Ok(res);
    }
}
