using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class User
{
    [Key]
    public string UuID { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}