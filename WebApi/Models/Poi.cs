using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class Poi
{
    [Key]
    public Guid UUID { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; }


    public Poi()
    {
        
    }

}