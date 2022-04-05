using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Models;

public class SwaggerSchemaExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo == null) return;
        if (context.MemberInfo.GetCustomAttributes(true)
                .FirstOrDefault() is SwaggerSchemaExampleAttribute schemaAttribute) ApplySchemaAttribute(schema, schemaAttribute);
    }

    private void ApplySchemaAttribute(OpenApiSchema schema, SwaggerSchemaExampleAttribute schemaAttribute)
    {
        schema.Example = new OpenApiString(schemaAttribute.Example);
    }
}
