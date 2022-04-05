namespace WebApi.Models;

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Enum)]
public class SwaggerSchemaExampleAttribute : Attribute
{
    public SwaggerSchemaExampleAttribute(string example)
    {
        Example = example;
    }

    public string Example { get; set; }
}
