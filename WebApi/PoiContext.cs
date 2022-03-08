using Microsoft.EntityFrameworkCore;
namespace WebApi;

public class PoiContext : DbContext
{
    public PoiContext()
        :base("PoiDB")
    {
        var a = true;
    }
}