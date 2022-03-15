using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Models;

[SwaggerSchema()]
public class Login
{
    [SwaggerSchema("Username", Nullable = false)]
    [SwaggerSchemaExample("testuser")]
    public string Username { get; set; }

    [SwaggerSchema("Password", Nullable = false)]
    [SwaggerSchemaExample("TestPassword123")]
    public string Password { get; set; }
}