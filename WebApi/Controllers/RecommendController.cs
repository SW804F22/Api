using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;
using WebApi.Models.DTOs;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RecommendController : ControllerBase
{
    public RecommendController(RecommenderService recommender, SearchService search, PoiContext context)
    {
        _recommender = recommender;
        _search = search;
        _context = context;
    }
    private readonly RecommenderService _recommender;
    private readonly SearchService _search;
    private readonly PoiContext _context;
    [HttpPost]
    [SwaggerOperation("Get Recommendation", "Get recommendation for user, based on location")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<PoiDTO>))]
    [SwaggerResponse(404, "Not found")]
    public async Task<ActionResult> Recommend([FromBody][SwaggerParameter("User and location to base the recommendation on")] Recommend parameters)
    {
        var user = await _context.Users.FindAsync(parameters.UserID);
        if (user == null)
        {
            return NotFound("User not found");
        }
        var pois = _search.Range(parameters.Latitude, parameters.Longitude, parameters.Range);
        if (!pois.Any())
        {
            return NotFound("No PoI's found in area");
        }
        var res = await _recommender.PostRecommendation(parameters.UserID, pois);
        return Ok(res);
    }
}
