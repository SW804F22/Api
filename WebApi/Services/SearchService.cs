using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Services;

public class SearchService
{

    public SearchService(PoiContext context)
    {
        _context = context;
    }
    
    private readonly PoiContext _context;
    public IQueryable<Poi> Range(IQueryable<Poi> set, double lat, double lon, double range)
    {
        var temp = set.Select(x => new
        {
            x,
            dist = Math.Sqrt(Math.Pow(lat - x.Latitude, 2) + Math.Pow(lon - x.Longitude, 2))
        });
        var result = temp.Where(y => y.dist < range).OrderBy(x => x.dist).Select(z => z.x);
        return result;
    }

    public IQueryable<Poi> Range(double lat, double lon, double range)
    {
        var result = _context.Pois
            .AsNoTracking()
            .Include(p => p.Categories)
            .AsQueryable();
        return Range(result, lat, lon, range);
    }
}
