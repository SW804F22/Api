using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebApi;
using WebApi.Models;
using WebApi.Models.DTOs;
using WebApi.Services;

namespace Test;

public class FakeSignInManager : SignInManager<User>
{
    public FakeSignInManager(PoiContext context)
        : base(new FakeUserManager(context),
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<User>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<User>>().Object)
    {
    }

    public override Task<SignInResult> PasswordSignInAsync(User user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var hasher = new PasswordHasher<User>();
        var res = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return Task.FromResult(res == PasswordVerificationResult.Success ? SignInResult.Success : SignInResult.Failed);
    }
}



public class FakeUserManager : UserManager<User>
{

    private readonly PoiContext _context;

    public FakeUserManager(PoiContext context)
        : base(new Mock<IUserStore<User>>().Object,
            new OptionsWrapper<IdentityOptions>(new IdentityOptions
            {
                Password = new PasswordOptions()
                {
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = true,
                    RequiredLength = 8,
                    RequiredUniqueChars = 1
                }
            }),
            new Mock<IPasswordHasher<User>>().Object,
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<User>>>().Object)
    {
        _context = context;
    }

    public override Task<User> FindByNameAsync(string userName)
    {
        return _context.Users.FirstAsync(u => u.UserName == userName);
    }

    public override async Task<IdentityResult> CreateAsync(User user, string password)
    {
        var validator = new PasswordValidator<User>();

        var val = await validator.ValidateAsync(this, user, password);
        if (val.Succeeded)
        {
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }
        else
        {
            return IdentityResult.Failed();
        }
    }

    public override Task<User> FindByIdAsync(string userId)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public override async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var validator = new PasswordValidator<User>();
        var val = await validator.ValidateAsync(this, user, newPassword);
        if (val.Succeeded)
        {
            var hasher = new PasswordHasher<User>();
            var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verify == PasswordVerificationResult.Success)
            {
                var hash = hasher.HashPassword(user, newPassword);
                _context.Update(user);
                user.PasswordHash = hash;
                var res = await _context.SaveChangesAsync();
                if (res == 1)
                {
                    return IdentityResult.Success;
                }
            }
        }
        return IdentityResult.Failed();
    }
}

public class MockRecommenderService : RecommenderService
{
    public MockRecommenderService() : base(new Mock<HttpClient>().Object)
    {
    }

    public override Task<IEnumerable<PoiDTO>> PostRecommendation(string user, IEnumerable<Poi> list)
    {
        var rng = new Random();
        var result = list.OrderBy(a => rng.Next()).ToList();
        return Task.FromResult(result.Select(p => new PoiDTO(p)).ToArray().AsEnumerable());
    }
}

