using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;
using School.Shared.Core.Abstractions.Options;
using TokenOptions = School.Exam.SIUSS.Services.Authentication.Models.Options.TokenOptions;

namespace School.Exam.SIUSS.Services.Authentication.Services;

public interface ITokenService
{
    public Task<string> Create(ApplicationUser entity);
}

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenOptions _tokenOptions;
    private readonly AuthOptions _authOptions;
    private readonly ILogger<TokenService> _logger;

    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(UserManager<ApplicationUser> userManager, IOptions<TokenOptions> tokenOptions, IOptions<AuthOptions> authOptions,
        ILogger<TokenService> logger)
    {
        _userManager = userManager;
        _tokenOptions = tokenOptions.Value;
        _authOptions = authOptions.Value;
        _logger = logger;

        _tokenHandler ??= new();
    }

    public async Task<string> Create(ApplicationUser entity)
    {
        var claims = await _userManager.GetClaimsAsync(entity);

        var accessToken = CreateToken(claims);
        
        _logger.LogInformation("{username} with id: {id} successfully logged in!", entity.UserName, entity.Id);

        return _tokenHandler.WriteToken(accessToken);
    }

    private JwtSecurityToken CreateToken(IList<Claim> claims)
    {
        var expiresIn = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_tokenOptions.LifeTimeInMinutes));
        const string algorithm = "HS256";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Secret));
        var signInCredentials = new SigningCredentials(signingKey, algorithm);

        return new
        (
            issuer: _authOptions.Issuer,
            audience: _authOptions.Audience,
            claims: claims,
            expires: expiresIn,
            signingCredentials: signInCredentials
        );
    }
}