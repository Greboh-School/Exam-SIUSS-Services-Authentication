using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Core;
using School.Exam.SIUSS.Services.Authentication.Models.DTOs;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;
using School.Exam.SIUSS.Services.Authentication.Models.Requests;
using School.Exam.SIUSS.Services.Authentication.Services;
using School.Shared.Core.Abstractions.Exceptions;
using TestPSchool.Exam.SIUSS.Services.Authentication.UnitTests.Setup;
using Xunit;

namespace TestPSchool.Exam.SIUSS.Services.Authentication.UnitTests.Services;

public class IdentityServiceTests : TestBase
{
    private readonly IIdentityService _uut;
    private readonly UserManager<ApplicationUser> _userManagerMock;
    private readonly SignInManager<ApplicationUser> _signInManagerMock;

    public IdentityServiceTests()
    {
        _userManagerMock = Util.CreateUserManagerMock<ApplicationUser>();

        _signInManagerMock = Util.CreateSignInManager(_userManagerMock);
        var loggerMock = Substitute.For<ILogger<IdentityService>>();

        _uut = new IdentityService(_userManagerMock, _signInManagerMock, loggerMock);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesUserInDatabase()
    {
        // Arrange
        const string username = "Tester";
        const string password = "Test-1234";
        const string createdBy = "TEST";

        var request = new CreateIdentityRequest()
        {
            UserName = username,
            Password = password,
            CreatedBy = createdBy
        };

        var expectation = new ApplicationUserDTO(Guid.NewGuid(), username);

        // This is necessary because the userManager's CreateAsync adds and calls SaveChanges() but we are mocking it therefor we need to do it
        async void AddToDatabase(CallInfo user)
        {
            await Context.ApplicationUsers.AddAsync(user.Arg<ApplicationUser>());
            await Context.SaveChangesAsync();
        }

        _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(Task.FromResult(IdentityResult.Success))
            .AndDoes(AddToDatabase);

        // This is likewise called by CreateAsync.
        _userManagerMock.AddClaimsAsync(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<Claim>>())
            .Returns(Task.FromResult(IdentityResult.Success));

        // Act
        var result = await _uut.Create(request);

        // Assert
        Context.ApplicationUsers.Should().ContainSingle();
        result.Should().BeEquivalentTo(expectation, opt =>
            opt.Excluding(x => x.UserId));
    }

    [Fact]
    public async Task Create_InvalidRequestUsernameTooShort_ThrowsBadRequest()
    {
        // Arrange
        const string username = "Test";
        const string password = "Test-1234";
        const string createdBy = "TEST";

        var request = new CreateIdentityRequest()
        {
            UserName = username,
            Password = password,
            CreatedBy = createdBy
        };

        // Act
        var result = async () => await _uut.Create(request);

        // Assert
        await result.Should()
            .ThrowExactlyAsync<BadRequestException>()
            .WithMessage("Invalid Username");
    }

    [Fact]
    public async Task Create_InvalidRequestPasswordNotAllowed_ThrowsBadRequest()
    {
        // Arrange
        const string username = "Tester";
        const string password = "test";
        const string createdBy = "TEST";
        const string errorReason = "InvalidPassword";
        var request = new CreateIdentityRequest()
        {
            UserName = username,
            Password = password,
            CreatedBy = createdBy
        };

        _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(Task.FromResult(IdentityResult.Failed([new() { Description = errorReason }])));

        // Act
        var result = async () => await _uut.Create(request);

        // Assert
        await result.Should()
            .ThrowExactlyAsync<BadRequestException>()
            .WithMessage("Failed to create ApplicationUser with username: Tester because: InvalidPassword");
    }

    [Fact]
    public async Task Get_UserWithIdExists_ReturnsApplicationUserDTO()
    {
        // Arrange
        var user = new ApplicationUser()
        {
            UserId = Guid.NewGuid(),
            UserName = "Tester"
        };
        Context.ApplicationUsers.Add(user);
        await Context.SaveChangesAsync();

        _userManagerMock.Users.Returns(Context.ApplicationUsers);

        var expectation = new ApplicationUserDTO(user.UserId, user.UserName);

        // Act
        var result = await _uut.GetByUserId(user.UserId);

        // Assert
        result.Should().BeEquivalentTo(expectation);
    }

    [Fact]
    public async Task Get_UserWithIdDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var user = new ApplicationUser()
        {
            UserId = Guid.NewGuid(),
            UserName = "Tester"
        };

        _userManagerMock.Users.Returns(Context.ApplicationUsers);

        // Act
        var result = async () => await _uut.GetByUserId(user.UserId);

        // Assert
        await result.Should()
            .ThrowExactlyAsync<NotFoundException>()
            .WithMessage($"Failed to retrieve user with userId {user.UserId}");
    }

    [Fact]
    public async Task Authorize_UserExists_ReturnsApplicationUser()
    {
        // Arrange
        const string username = "Tester";
        const string password = "Test-1234";
        var user = new ApplicationUser()
        {
            UserId = Guid.NewGuid(),
            UserName = username
        };
        Context.ApplicationUsers.Add(user);
        await Context.SaveChangesAsync();

        _userManagerMock.FindByNameAsync(username)
            .Returns(user);

        _signInManagerMock.CheckPasswordSignInAsync(user, password, false)
            .Returns(SignInResult.Success);

        var request = new CreateUserSessionRequest(username, password);

        // Act
        var result = await _uut.Authorize(request);

        // Assert
        result.Should().Be(user);
    }

    [Fact]
    public async Task Authorize_UserDoesNotExists_ThrowsNotFoundException()
    {
        // Arrange
        const string username = "Tester";
        const string password = "Test-1234";

        _userManagerMock.FindByNameAsync(username)
            .Returns(null as ApplicationUser);

        var request = new CreateUserSessionRequest(username, password);

        // Act
        var result = async () => await _uut.Authorize(request);

        // Assert
        await result.Should()
            .ThrowExactlyAsync<NotFoundException>()
            .WithMessage($"Failed to find user with username: {request.UserName}");
    }
}