using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;

namespace WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class PoiController: ControllerBase
{
    private PoiContext _context;

    public PoiController(PoiContext context)
    {
        _context = context;
    }


    [HttpGet("id", Name = "GetPoi")]
    [SwaggerOperation(Summary = "Gets a Poi", Description = "Gets Poi from id")]
    [SwaggerResponse(200, "Success", typeof(Poi))]
    [SwaggerResponse(404, "Poi not found")]
    public async Task<ActionResult> GetPoi([SwaggerParameter("Id of Poi", Required = true)]Guid id)
    {
        var p = await _context.Pois.FindAsync(id);

        if (p == null)
        {
            return NotFound($"Poi with id {id} not found");
        }

        return Ok(p);
    }

    private class Point
    {
        private double lattitude;
        private double longitude;

        public Point(double? lat, double? lon)
        {
            lattitude = lat ?? 0;
            longitude = lon ?? 0;
        }

        public double Distance(Poi p)
        {
            return Math.Sqrt(Math.Pow(lattitude - p.Latitude, 2) + Math.Pow(longitude - p.Longitude, 2));
        }
    }

    [HttpGet]
    [Route("search/")]
    public async Task<ActionResult> Search([FromQuery] string? name, [FromQuery] string? category, [FromQuery] double? latitude, [FromQuery] double? longitude, [FromQuery] double? distance)
    {
        var result = _context.Pois.AsEnumerable();
        
        if (latitude != null && longitude != null && distance != null)
        {
            var loc = new Point(latitude, longitude);
            var temp = result.Select(x => new { x.UUID, dist = loc.Distance(x) });
            var keys = temp.Where(y => y.dist < distance).Select(z => z.UUID);
            result = result.IntersectBy(keys, a => a.UUID);
        }
        else if (latitude != null || longitude != null || distance != null)
        {
            return BadRequest("Latitude, longitude and distance required together");
        }
        
        if (name != null)
        {
            result = result.Where(p=> p.Title.Contains(name));
        }

        if (category != null)
        {
            var cat = await _context.Categories.FirstAsync(c => c.Name == category);
            result = result.Where(p => p.Categories.Contains(cat));
        }

        return Ok(result);
    }
    
    private async Task<Poi> FromDTO(PoiDTO dto)
    {
        var p = new Poi();
        p.Title = dto.Title;
        p.Latitude = dto.Latitude;
        p.Longitude = dto.Longitude;
        p.Description = dto.Description;
        p.Website = dto.Website;
        p.PriceStep = dto.PriceStep;
        p.Address = dto.Address;
        p.Categories = new List<Category>();
        foreach (var cat in dto.Categories)
        {
            try
            {
                var res = await _context.Categories.Include(x => x.Parent)
                    .ThenInclude(y=> y.Parent).FirstAsync(c => c.Name == cat);
                p.Categories.Add(res);
                while (res.Parent != null)
                {
                    res = res.Parent;
                    if(!p.Categories.Contains(res))
                        p.Categories.Add(res);
                }
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidDataException(cat, e);
            }
        }
        return p;
    }

    [HttpPost]
    public async Task<ActionResult> CreatePoi([FromBody] PoiDTO p)
    {
        Poi? newp = null;
        string message = "";
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
            if (result >= 1)
            {
                return Created("Success", newp);
            }
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
        if (p == null)
        {
            return NotFound($"Poi with id {id} not found");
        }

        _context.Pois.Update(p);
        p.Title = poi.Title;
        p.Latitude = poi.Latitude;
        p.Longitude = poi.Longitude;
        p.Description = poi.Description;
        var result = await _context.SaveChangesAsync();
        if (result != 1)
        {
            return Conflict("An error occurred while updating");
        }

        return Ok("Update success");
    }

    [HttpDelete("id")]
    public async Task<ActionResult> DeletePoi(Guid id)
    {
        var p = await _context.Pois.FindAsync(id);
        if (p == null)
        {
            return NotFound($"Poi with id {id} not found");
        }

        _context.Pois.Remove(p);
        var result = await _context.SaveChangesAsync();
        if (result != 1)
        {
            return BadRequest("An unexpected error occured");
        }

        return Ok("Successful deletion");

    }
}