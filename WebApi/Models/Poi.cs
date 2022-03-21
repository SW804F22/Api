using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public enum Price
{
    Cheap,
    Moderate,
    Expensive,
    VeryExpensive
}
public class Poi
{
    [Key]
    public Guid? UUID { get; set; }
    public string Title { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; }
    public string Website { get; set; }
    public Price PriceStep { get; set; }
    
    public ICollection<Category> Categories { get; set; }


    public Poi()
    {
        
    }
    

}