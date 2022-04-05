using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PoiController : ControllerBase
{
    private readonly PoiContext _context;

    public PoiController(PoiContext context)
    {
        _context = context;
    }


    [HttpGet("id", Name = "GetPoi")]
    [SwaggerOperation(Summary = "Gets a Poi", Description = "Gets Poi from id")]
    [SwaggerResponse(200, "Success", typeof(Poi))]
    [SwaggerResponse(404, "Poi not found")]
    public async Task<ActionResult> GetPoi([SwaggerParameter("Id of Poi", Required = true)] Guid id)
    {
        try
        {
            var p = await _context.Pois.Include(p => p.Categories).FirstAsync(p => p.UUID == id);
            return Ok(p);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Poi with id {id} not found");
        }
    }

    [HttpGet]
    [Route("search/")]
    public async Task<IActionResult> Search(
        [FromQuery] string? name,
        [FromQuery] IEnumerable<string> category,
        [FromQuery] IEnumerable<string> notCategory,
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        [FromQuery] double? distance,
        [FromQuery] IEnumerable<Price> prices,
        [FromQuery] int limit = 50)
    {
        var result = _context.Pois
            .AsNoTracking()
            .Include(p => p.Categories)
            .AsQueryable();

        if (latitude is not null && longitude is not null && distance is not null)
        {
            var temp = result.Select(x => new
            {
                x,
                dist = Math.Sqrt(Math.Pow(latitude.Value - x.Latitude, 2) + Math.Pow(longitude.Value - x.Longitude, 2))
            });
            result = temp.Where(y => y.dist < distance).OrderBy(x => x.dist).Select(z => z.x);
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

        return Ok(result.Take(limit));
    }

    private async Task<Poi> FromDTO(PoiDTO dto)
    {
        var p = new Poi(dto.Title, dto.Latitude, dto.Longitude, dto.Description, dto.Website, dto.Address, dto.PriceStep);

        if (dto.Categories == null) throw new InvalidDataException();
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
    public async Task<ActionResult> CreatePoi([FromBody] PoiDTO p)
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
            if (result >= 1) return Created("Success", newp);
        }
        else
        {
            message = "At least one category required";
        }

        return BadRequest("A un expected error occured." + message);
    }

    [HttpPut("id")]
    public async Task<ActionResult> EditPoi(Guid id, [FromBody] Poi poi)
    {
        var p = await _context.Pois.FindAsync(id);
        if (p == null) return NotFound($"Poi with id {id} not found");

        _context.Pois.Update(p);
        p.Title = poi.Title;
        p.Latitude = poi.Latitude;
        p.Longitude = poi.Longitude;
        p.Description = poi.Description;
        var result = await _context.SaveChangesAsync();
        if (result != 1) return Conflict("An error occurred while updating");

        return Ok("Update success");
    }

    [HttpDelete("id")]
    public async Task<ActionResult> DeletePoi(Guid id)
    {
        var p = await _context.Pois.FindAsync(id);
        if (p == null) return NotFound($"Poi with id {id} not found");

        _context.Pois.Remove(p);
        var result = await _context.SaveChangesAsync();
        if (result != 1) return BadRequest("An unexpected error occured");

        return Ok("Successful deletion");
    }
}
