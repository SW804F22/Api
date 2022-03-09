using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class Poi
{
    [Key]
    public string UuID { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }


    public Poi()
    {
        
    }

}