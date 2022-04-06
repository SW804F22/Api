using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class Checkin
{
    public Checkin()
    {
        UUID = Guid.NewGuid();
    }
    public Checkin(Poi p, DateTime t)
    {
        Point = p;
        Timestamp = t;
        UUID = Guid.NewGuid();
    }
    [Key] public Guid UUID { get; set; }
    public Poi? Point { get; set; }
    public DateTime Timestamp { get; set; }
}
