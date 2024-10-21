using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;
using School.Shared.Core.Abstractions.Options;
using School.Shared.Core.Authentication.Claims;
using TokenOptions = School.Exam.SIUSS.Services.Authentication.Models.Options.TokenOptions;

namespace TestPSchool.Exam.SIUSS.Services.Authentication.UnitTests.Setup;

public static class Util
{
    public static SignInManager<T> CreateSignInManager<T>(UserManager<T> userManager) where T : class
    {
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<T>>();

        return Substitute.For<SignInManager<T>>(userManager, contextAccessor, claimsFactory, null, null, null, null);
    }

    public static UserManager<T> CreateUserManagerMock<T>() where T : class
    {
        var store = Substitute.For<IUserStore<T>>();
        var manager = Substitute.For<UserManager<T>>(store, null, null, null, null, null, null, null, null);

        return manager;
    }

    public static List<Claim> CreateClaims(ApplicationUser entity)
    {
        var claims = new List<Claim>
        {
            new("iid", entity.Id.ToString()),
            new("uid", entity.UserId.ToString()),
            new("sub", entity.UserName!),
            new("systems:website:role", Enum.GetName(ClaimLevel.User)!),
            new("systems:game:role", Enum.GetName(ClaimLevel.User)!)
        };

        return claims;
    }
    
    public static JwtSecurityToken CreateToken(List<Claim> claims, AuthOptions authOptions, TokenOptions tokenOptions)
    {
        var expiresIn = DateTime.UtcNow.Add(TimeSpan.FromMinutes(tokenOptions.LifeTimeInMinutes));
        const string algorithm = "HS256";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Secret));
        var signInCredentials = new SigningCredentials(signingKey, algorithm);

        return new
        (
            issuer: authOptions.Issuer,
            audience: authOptions.Audience,
            claims: claims,
            expires: expiresIn,
            signingCredentials: signInCredentials
        );
    }
}