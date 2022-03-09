using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class Checkin
{
    [Key]
    public string UuID { get; set; }
    public User User { get; set; }
    public Poi Point { get; set; }
    public DateTime Timestamp { get; set; }
}