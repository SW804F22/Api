using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Models;
[SwaggerSchema("User information")]
public class UserDTO
{

    public UserDTO(User user)
    {
        Id = user.Id;
        UserName = user.UserName;
        DateOfBirth = user.DateOfBirth.Value;
        Gender = user.Gender;
    }
    [SwaggerSchema("User id")]
    [SwaggerSchemaExample("9b279878-74fd-46c4-8980-307f80375723")]
    public string Id { get; set; }
    [SwaggerSchema("Username")]
    [SwaggerSchemaExample("TestUser12321")]
    public string UserName { get; set; }
    [SwaggerSchema("Date of birth")]
    [SwaggerSchemaExample("1991-02-25")]
    public DateTime DateOfBirth { get; set; }
    [SwaggerSchema("Gender [Unspecified, Male, Female]")]
    [SwaggerSchemaExample("2")]
    public Gender Gender { get; set; }
}
