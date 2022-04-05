using Microsoft.AspNetCore.Identity;

namespace WebApi.Models;

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
