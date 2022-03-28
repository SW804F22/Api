using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class Checkin
{
    [Key]
    public Guid UUID { get; set; }
    public Poi Point { get; set; }
    public DateTime Timestamp { get; set; }
}