using System.Diagnostics.CodeAnalysis;

namespace WebApi.Models;

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Enum)]
[ExcludeFromCodeCoverage]
public class SwaggerSchemaExampleAttribute : Attribute
{
    public SwaggerSchemaExampleAttribute(string example)
    {
        Example = example;
    }

    public string Example { get; set; }
}
