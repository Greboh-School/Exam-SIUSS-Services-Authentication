using System.IdentityModel.Tokens.Jwt;
using System.Net.NetworkInformation;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;
using School.Exam.SIUSS.Services.Authentication.Services;
using School.Shared.Core.Abstractions.Options;
using TestPSchool.Exam.SIUSS.Services.Authentication.UnitTests.Setup;
using Xunit;
using TokenOptions = School.Exam.SIUSS.Services.Authentication.Models.Options.TokenOptions;

namespace TestPSchool.Exam.SIUSS.Services.Authentication.UnitTests.Services;

public class TokenServiceTests : TestBase
{
    private readonly ITokenService _uut;
    private readonly UserManager<ApplicationUser> _userManagerMock;
    private readonly ILogger<TokenService> _loggerMock;
    private readonly IOptions<TokenOptions> _tokenOptions;
    private readonly IOptions<AuthOptions> _authOptions;

    public TokenServiceTests()
    {
        _userManagerMock = Util.CreateUserManagerMock<ApplicationUser>();
        _loggerMock = Substitute.For<ILogger<TokenService>>();

        _tokenOptions = Options.Create(new TokenOptions
        {
            LifeTimeInMinutes = 5
        });
        
        _authOptions = Options.Create(new AuthOptions
        {
            Secret = "Superduperlongtestsecretthatshouldnotbepublic",
            Issuer = "Issuer",
            Audience = "Audience"
        });

        _uut = new TokenService(_userManagerMock, _tokenOptions, _authOptions, _loggerMock);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesSession()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "Tester",
            UserId = Guid.NewGuid(),
        };
        var claims = Util.CreateClaims(user);
        var accessToken = Util.CreateToken(claims, _authOptions.Value, _tokenOptions.Value);
        
        var expectation = new JwtSecurityTokenHandler().WriteToken(accessToken);
        
        _userManagerMock.GetClaimsAsync(Arg.Any<ApplicationUser>())
            .Returns(claims);
        
        // Act
        var result = await _uut.Create(user);
        
        // Assert
        result.Should().BeEquivalentTo(expectation);
    }
}