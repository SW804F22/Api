using Duende.IdentityServer.Extensions;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;
using WebApi.Models.DTOs;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PoiController : ControllerBase
{
    private readonly PoiContext _context;
    private readonly SearchService _search;

    public PoiController(PoiContext context, SearchService search)
    {
        _context = context;
        _search = search;
    }


    [HttpGet("id", Name = "GetPoi")]
    [SwaggerOperation(Summary = "Gets a Poi", Description = "Gets Poi from id")]
    [SwaggerResponse(200, "Success", typeof(PoiDTO))]
    [SwaggerResponse(404, "Poi not found")]
    public async Task<ActionResult> GetPoi([SwaggerParameter("Id of Poi", Required = true)] Guid id)
    {
        try
        {
            var p = await _context.Pois.Include(p => p.Categories).FirstAsync(p => p.UUID == id);
            return Ok(new PoiDTO(p));
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Poi with id {id} not found");
        }
    }

    [HttpGet]
    [Route("search/")]
    [SwaggerOperation(Summary = "Search for PoI's", Description = "Search PoI's using any combination of name, category, unwanted category, price, location and range")]
    [SwaggerResponse(200, "Success", typeof(PoiDTO[]))]
    [SwaggerResponse(404, "No Poi's matching criteria found or Category not found")]
    [SwaggerResponse(400, "Invalid arguments")]
    public async Task<IActionResult> Search(
        [FromQuery][SwaggerParameter("Name")] string? name,
        [FromQuery][SwaggerParameter("List of categories to include in search")] IEnumerable<string> category,
        [FromQuery][SwaggerParameter("List of categories not to include in search")] IEnumerable<string> notCategory,
        [FromQuery][SwaggerParameter("Location latitude")] double? latitude,
        [FromQuery][SwaggerParameter("Location longitude")] double? longitude,
        [FromQuery][SwaggerParameter("Range distance from location to include in search")] double? distance,
        [FromQuery][SwaggerParameter("List of price steps to include")] IEnumerable<Price> prices,
        [FromQuery][SwaggerParameter("Maximum number of results")] int limit = 50)
    {
        var result = _context.Pois
            .AsNoTracking()
            .Include(p => p.Categories)
            .AsQueryable();

        if (latitude is not null && longitude is not null && distance is not null)
        {
            result = _search.Range(result, latitude.Value, longitude.Value, distance.Value);
        }
        else if (latitude != null || longitude != null || distance != null)
        {
            return BadRequest("Latitude, longitude and distance required together");
        }

        if (prices.Any()) result = result.Where(p => prices.Contains(p.PriceStep));

        if (name != null) result = result.Where(p => p.Title.Contains(name));

        if (category.Any())
        {
            foreach (var cat in category)
                try
                {
                    await _context.Categories.FirstAsync(c => c.Name == cat);
                }
                catch (InvalidOperationException)
                {
                    return NotFound("Category " + cat + " could not be found");
                }

            result = result.Where(p => p.Categories.Any(x => category.Contains(x.Name)));
        }

        if (notCategory.Any())
        {
            foreach (var cat in notCategory)
                try
                {
                    await _context.Categories.FirstAsync(c => c.Name == cat);
                }
                catch (InvalidOperationException)
                {
                    return NotFound("Category " + cat + " could not be found");
                }

            result = result.Where(p => p.Categories.All(x => !notCategory.Contains(x.Name)));
        }

        if (!result.Any())
        {
            return NotFound("No Poi's matching criteria");
        }

        return Ok(result.Take(limit).Select(p => new PoiDTO(p)).ToArray());
    }
    [HttpGet]
    [Route("search/name/{query}")]
    [SwaggerOperation("Search for Poi name", "Get suggestions on the names of pois")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<String>))]
    public ActionResult SearchName([SwaggerParameter("Search string")] string query)
    {
        var pois = _context.Pois.AsNoTracking().Select(p => p.Title).Distinct();
        var result = Process.ExtractSorted(query, pois, s => s, ScorerCache.Get<PartialTokenSetScorer>());
        return Ok(result.Select(p => p.Value).Take(20).ToArray());
    }

    /// <summary>
    /// Constructs a PoI from a PoIDTO
    /// </summary>
    /// <param name="dto">DTO object to construct from</param>
    /// <returns>Poi object coresponding to the dto object passed</returns>
    /// <exception cref="InvalidDataException">Categories not found</exception>
    private async Task<Poi> FromDTO(PoiDTO dto)
    {
        var p = new Poi(dto.Title, dto.Latitude, dto.Longitude, dto.Description, dto.Website, dto.Address, dto.PriceStep);
        // Categories cannot be null because it is checked before method is called
        foreach (var cat in dto.Categories)
            try
            {
                var res = await _context.Categories.Include(x => x.Parent)
                    .ThenInclude(y => y.Parent).FirstAsync(c => c.Name == cat);
                p.Categories.Add(res);
                while (res.Parent != null)
                {
                    res = res.Parent;
                    if (!p.Categories.Contains(res))
                        p.Categories.Add(res);
                }
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidDataException(cat, e);
            }

        return p;
    }

    [HttpPost]
    [SwaggerOperation("Create PoI", "Create a new PoI with necessary information")]
    [SwaggerResponse(201, "Creation success", typeof(PoiDTO))]
    [SwaggerResponse(404, "Category not found")]
    [SwaggerResponse(400, "At least one category required")]
    public async Task<ActionResult> CreatePoi([FromBody][SwaggerRequestBody("DTO holding the information of the new PoI")] PoiDTO p)
    {
        Poi? newp = null;
        var message = "";
        if (!p.Categories.IsNullOrEmpty())
        {
            try
            {
                newp = (await _context.Pois.AddAsync(await FromDTO(p))).Entity;
            }
            catch (InvalidDataException e)
            {
                return NotFound("Category not found. " + e.Message);
            }

            var result = await _context.SaveChangesAsync();
            if (result >= 1) return Created("Success", new PoiDTO(newp));
        }
        else
        {
            message = "At least one category required";
        }

        return BadRequest("A un expected error occured." + message);
    }

    [HttpPut("id")]
    [SwaggerOperation("Edit PoI", "Edit the information of a PoI")]
    [SwaggerResponse(200, "Update success")]
    [SwaggerResponse(404, "PoI not found")]
    [SwaggerResponse(409, "Cannot update")]
    public async Task<ActionResult> EditPoi([SwaggerParameter("Id of PoI to update")] Guid id, [FromBody][SwaggerRequestBody("DTO object containing information to update")] PoiDTO poi)
    {
        var p = await _context.Pois.FindAsync(id);
        if (p == null) return NotFound($"Poi with id {id} not found");

        _context.Pois.Update(p);
        p.Title = poi.Title;
        p.Latitude = poi.Latitude;
        p.Longitude = poi.Longitude;
        p.Description = poi.Description;
        p.Website = poi.Website;
        p.Address = poi.Address;
        p.PriceStep = poi.PriceStep;
        var result = await _context.SaveChangesAsync();
        if (result != 1) return Conflict("An error occurred while updating");

        return Ok("Update success");
    }

    [HttpDelete("id")]
    [SwaggerOperation("Delete Poi", "Delete the PoI with id")]
    [SwaggerResponse(200, "Successful deletion")]
    [SwaggerResponse(404, "PoI not found")]
    [SwaggerResponse(400, "An error occurred")]
    public async Task<ActionResult> DeletePoi([SwaggerParameter("Id of PoI to delete")] Guid id)
    {
        var p = await _context.Pois.FirstOrDefaultAsync(p => p.UUID == id);
        if (p == null) return NotFound($"Poi with id {id} not found");
        _context.Pois.Remove(p);
        var result = await _context.SaveChangesAsync();
        if (result < 1) return BadRequest("An unexpected error occured");
        return Ok("Successful deletion");
    }

    [HttpGet]
    [Route("Category")]
    [SwaggerOperation("Search for category", "Search for categories by name")]
    [SwaggerResponse(200, "Success", typeof(string[]))]
    public ActionResult SearchCategory([FromQuery][SwaggerParameter("Search string")] string? query, [FromQuery][SwaggerParameter("Amount of results")] int limit = 50)
    {
        if (query == null)
        {
            return Ok(_context.Categories.Select(c => c.Name).ToArray());
        }
        var categories = _context.Categories.Select(c => c.Name);
        var result = Process.ExtractSorted(query, categories, s => s, ScorerCache.Get<TokenSetScorer>());
        return Ok(result.Select(c => c.Value).Take(limit).ToArray());
    }
}
