using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Models;
[SwaggerSchema("Gender")]
public enum Gender
{
    Unspecified,
    Male,
    Female
}

public class User : IdentityUser
{
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.Unspecified;

    public ICollection<Checkin>? Checkins { get; set; }
}
