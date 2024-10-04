using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using School.Exam.SIUSS.Services.Authentication.IntegrationTests.Setup;
using School.Exam.SIUSS.Services.Authentication.Models.DTOs;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;
using School.Exam.SIUSS.Services.Authentication.Models.Requests;
using School.Shared.Tools.Test.Extensions;
using Xunit;

namespace School.Exam.SIUSS.Services.Authentication.IntegrationTests.Controllers;

public class SessionsControllerTests : TestBase
{
    private readonly string _baseAddress = "api/v1/sessions";
    
    public SessionsControllerTests(ApiWebApplicationFactory webApplicationFactory) : base(webApplicationFactory)
    {
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsOkAndCreatesSession()
    {
        // Arrange
        const string username = "Tester";
        const string password = "Test-1234";

        var body = new CreateUserSessionRequest(username, password);
        
        var entity = new ApplicationUser
        {
            UserName = username,
        };

        await UserManager.CreateAsync(entity, password);
        
        // Act
        var response = await Client.PostAsJsonAsync($"{_baseAddress}", body);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.ReadAsAsync<UserSessionDTO>();
        
        result.Should().NotBeNull();
        result.UserName.Should().BeEquivalentTo(username);
        result.AccessToken.Should().NotBeNullOrEmpty();
    }
}