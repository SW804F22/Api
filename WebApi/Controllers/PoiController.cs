using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
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

    [HttpGet]
    [Route("search")]
    public async Task<ActionResult> Search()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<ActionResult> CreatePoi([FromBody] Poi p)
    {
        p.UUID = null;
        var newp = await _context.Pois.AddAsync(p);
        var result = await _context.SaveChangesAsync();
        if (result == 1)
        {
            return Created("Success",newp.Entity);
        }

        return BadRequest("A un expected error occured");
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