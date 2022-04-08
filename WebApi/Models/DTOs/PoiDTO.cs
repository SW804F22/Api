using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Models.DTOs;

[SwaggerSchema("Point of interest")]
public class PoiDTO
{
    public PoiDTO(Poi p)
    {
        id = p.UUID;
        Title = p.Title;
        Latitude = p.Latitude;
        Longitude = p.Longitude;
        Description = p.Description;
        Address = p.Address;
        PriceStep = p.PriceStep;
        Categories = p.Categories.Select(c => c.Name).ToList();
    }
    public PoiDTO() { }
    public Guid? id { get; set; }

    [SwaggerSchema("Title of PoI", Nullable = true)]
    [SwaggerSchemaExample("Absalon Hotel")]
    public string Title { get; set; } = "";
    [SwaggerSchema("Location latitude", Nullable = true)]
    [SwaggerSchemaExample("55.671565")]
    public double Latitude { get; set; } = 0.0;
    [SwaggerSchema("Location longitude", Nullable = true)]
    [SwaggerSchemaExample("12.561658")]
    public double Longitude { get; set; } = 0.0;
    [SwaggerSchema("Description of the PoI", Nullable = true)]
    [SwaggerSchemaExample("Newly renovated family owned hotel in trendy Vesterbro. Next to Meatpacking district, cafées, bars and designer shops")]
    public string Description { get; set; } = "";
    [SwaggerSchema("Website for the PoI")]
    [SwaggerSchemaExample("http://www.absalon-hotel.dk")]
    public string Website { get; set; } = "";
    [SwaggerSchema("The address of the PoI")]
    [SwaggerSchemaExample("Helgolandsgade 15 (Istedgade), 1653 København, DK")]
    public string Address { get; set; } = "";
    [SwaggerSchema("The price range of the PoI", Nullable = true)]
    [SwaggerSchemaExample("0")]
    public Price PriceStep { get; set; } = Price.Free;
    [SwaggerSchema("The list of categories that the PoI belongs to")]
    public List<string>? Categories { get; set; }
}
