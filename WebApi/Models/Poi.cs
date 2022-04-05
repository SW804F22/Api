using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace WebApi.Models;

public enum Price
{
    Free,
    Cheap,
    Moderate,
    Expensive,
    VeryExpensive
}

public class Poi
{

    public Poi(string title, double lat, double lon, string description, string website, string address, Price price)
    {
        Title = title;
        Latitude = lat;
        Longitude = lon;
        Description = description;
        Website = website;
        UUID = Guid.NewGuid();
        Address = address;
        PriceStep = price;
        Categories = new List<Category>();
    }
    [Key] public Guid? UUID { get; set; }
    public string Title { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; }
    public string Website { get; set; }
    public string Address { get; set; }
    public Price PriceStep { get; set; }
    public ICollection<Category>? Categories { get; set; }
}
