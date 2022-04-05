namespace WebApi.Models;

public class Recommend
{
    public string? UserID { get; set; }
    public double? Lattitude { get; set; }
    public double? Longtitude { get; set; }
    public double? Range { get; set; }
}
