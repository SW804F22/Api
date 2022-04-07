using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Models.DTOs;
[SwaggerSchema()]
public class Recommend
{
    [SwaggerSchema("Id of user to recommend", Nullable = false)]
    [SwaggerSchemaExample("9b279878-74fd-46c4-8980-307f80375723")]
    public string UserID { get; set; } = "";
    [SwaggerSchema("Location latitude that is the center of the recommendation")]
    [SwaggerSchemaExample("55.6")]
    public double Latitude { get; set; } = 0.0;
    [SwaggerSchema("Location longitude that is the center of the recommendation")]
    [SwaggerSchemaExample("12.5")]
    public double Longitude { get; set; } = 0.0;
    [SwaggerSchema("Distance from the center to include recommendations from. 0.01 is approx 1 km")]
    [SwaggerSchemaExample("0.1")]
    public double Range { get; set; } = 0.0;
}
