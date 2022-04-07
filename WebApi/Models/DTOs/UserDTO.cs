namespace WebApi.Models;

public class UserDTO
{

    public UserDTO(User user)
    {
        Id = user.Id;
        UserName = user.UserName;
        DateOfBirth = user.DateOfBirth.Value;
        Gender = user.Gender;
    }

    public string Id { get; set; }
    public string UserName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
}
