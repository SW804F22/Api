using Microsoft.AspNetCore.Identity;
using WebApi.Models;

namespace WebApi.Services;

public class AuthenticateService
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthenticateService(SignInManager<User> signin, UserManager<User> user)
    {
        _signInManager = signin;
        _userManager = user;
    }

    public virtual async Task<(SignInResult, User)> Login(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        return (await _signInManager.PasswordSignInAsync(user, password, true, false), user);
    }

    public async Task<IdentityResult> Register(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<User?> FindUser(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<IdentityResult> ChangePassword(User user, string old, string newpass)
    {
        return await _userManager.ChangePasswordAsync(user, old, newpass);
    }
}
