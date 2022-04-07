using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Models;

[SwaggerSchema]
public class Login
{

    public Login(string u, string p)
    {
        Username = u;
        Password = p;
    }

    [SwaggerSchema("Username", Nullable = false)]
    [SwaggerSchemaExample("Testuser")]
    public string Username { get; set; }

    [SwaggerSchema("Password", Nullable = false)]
    [SwaggerSchemaExample("TestPassword123")]
    public string Password { get; set; }
}
