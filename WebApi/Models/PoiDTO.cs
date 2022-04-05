namespace WebApi.Models;

public class PoiDTO
{
    public string Title { get; set; } = "";
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
    public string Description { get; set; } = "";
    public string Website { get; set; } = "";
    public string Address { get; set; } = "";
    public Price PriceStep { get; set; } = Price.Free;

    public List<string>? Categories { get; set; }
}
