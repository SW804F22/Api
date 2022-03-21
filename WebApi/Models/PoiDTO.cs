namespace WebApi.Models;

public class PoiDTO
{
    public string Title { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; }
    public string Website { get; set; }
    public Price PriceStep { get; set; }

    public List<String> Categories { get; set; }
}