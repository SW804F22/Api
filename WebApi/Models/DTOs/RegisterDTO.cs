using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Models;
[SwaggerSchema("Registration information")]
public class Register
{
    public Register(string user, string pass, DateTime dob, Gender g)
    {
        Username = user;
        Password = pass;
        DateOfBirth = dob;
        Gender = g;
    }
    [SwaggerSchema("Username", Nullable = false)]
    [SwaggerSchemaExample("TestUser12321")]
    public string Username { get; set; }
    [SwaggerSchema("Password", Nullable = false)]
    [SwaggerSchemaExample("TestPassword123")]
    public string Password { get; set; }
    [SwaggerSchema("Date of birth")]
    [SwaggerSchemaExample("1990-01-01")]
    public DateTime DateOfBirth { get; set; }
    [SwaggerSchema("Gender [Unspecified, Male, Female]", Nullable = false)]
    [SwaggerSchemaExample("1")]
    public Gender Gender { get; set; }
}
