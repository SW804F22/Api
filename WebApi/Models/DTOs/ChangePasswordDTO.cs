using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Models.DTOs;

[SwaggerSchema("Password change")]
public class ChangePasswordDTO
{
    [SwaggerSchema("Old password for verification", Nullable = false)]
    [SwaggerSchemaExample("TestPassword123")]
    public string OldPassword { get; set; } = "";
    [SwaggerSchema("New password to set for user", Nullable = false)]
    [SwaggerSchemaExample("TestPassword321")]
    public string NewPassword { get; set; } = "";
}
