namespace WebApi.Models;

public class Register
{
    public string Username { get; set; }
    public string Password { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
}