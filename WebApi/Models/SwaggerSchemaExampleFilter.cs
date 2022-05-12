using System.Diagnostics.CodeAnalysis;
using System.Drawing.Printing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Models;

[ExcludeFromCodeCoverage]
public class SwaggerSchemaExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo != null && context.MemberInfo.GetCustomAttributes(typeof(SwaggerSchemaExampleAttribute), false).FirstOrDefault() is SwaggerSchemaExampleAttribute att)
        {
            ApplySchemaAttribute(schema, att);
        }

    }

    private void ApplySchemaAttribute(OpenApiSchema schema, SwaggerSchemaExampleAttribute schemaAttribute)
    {
        schema.Example = new OpenApiString(schemaAttribute.Example);
    }
}
