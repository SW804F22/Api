namespace WebApi.Models;

public class Category
{
    public int Id{ get; set; }
    public string Name { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category>? SubCategories { get; set; }
    public ICollection<Poi>? Locations { get; set; }
}

