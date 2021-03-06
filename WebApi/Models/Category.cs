namespace WebApi.Models;

public class Category
{
    public Category()
    {
        Name = "";
    }
    public Category(string name, Category? parent)
    {
        Name = name;
        Parent = parent;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category>? SubCategories { get; set; }
    public ICollection<Poi>? Locations { get; set; }
}
