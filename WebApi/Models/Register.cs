namespace WebApi.Models;

public class Register
{
    public Register(string user, string pass, DateTime dob, Gender g)
    {
        Username = user;
        Password = pass;
        DateOfBirth = dob;
        Gender = g;
    }
    public string Username { get; set; }
    public string Password { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
}
