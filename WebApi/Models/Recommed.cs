namespace WebApi.Models;

public class Recommend
{
    public string UserID { get; set; } = "";
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
    public double Range { get; set; } = 0.0;
}
