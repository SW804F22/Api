using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Models;

public class User : IdentityUser{

public int Age { get; set; }
    
}