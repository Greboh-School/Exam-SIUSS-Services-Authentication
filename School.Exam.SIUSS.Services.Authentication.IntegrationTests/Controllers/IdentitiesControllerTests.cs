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

public class IdentitiesControllerTests : TestBase
{
    private const string _baseAddress = "api/v1/identities";

    public IdentitiesControllerTests(ApiWebApplicationFactory webApplicationFactory) : base(webApplicationFactory)
    {
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsOkCreatedAndCreatesUser()
    {
        // Arrange
        const string username = "Tester";
        const string password = "Test-1234";
        const string createdBy = "TEST";
        
        var body = new CreateIdentityRequest
        {
            UserName = username,
            Password = password,
            CreatedBy = createdBy
        };

        var expectation = new ApplicationUserDTO(Guid.NewGuid(), username);
        
        // Act
        var response = await Client.PostAsJsonAsync($"{_baseAddress}", body);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Context.ApplicationUsers.Should().ContainSingle();
        
        var result = await response.ReadAsAsync<ApplicationUserDTO>();
        result.Should().BeEquivalentTo(expectation, opt =>
            opt.Excluding(x => x.UserId));
    }
    [Fact]
    public async Task CreateUser_InvalidRequestUsernameTooShort_ReturnsBadRequest()
    {
        // Arrange
        const string username = "Test";
        const string password = "Test-1234";
        const string createdBy = "TEST";
        
        var body = new CreateIdentityRequest
        {
            UserName = username,
            Password = password,
            CreatedBy = createdBy
        };
        
        // Act
        var response = await Client.PostAsJsonAsync($"{_baseAddress}", body);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Context.ApplicationUsers.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetByUserId_ValidRequest_ReturnsDTO()
    {
        // Arrange
        var query = Guid.NewGuid();
        var userName = "Tester";

        var entity = new ApplicationUser
        {
            UserId = query,
            UserName = "Tester",
        };

        var expectation = new ApplicationUserDTO(query, userName);

        Context.ApplicationUsers.Add(entity);
        await Context.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"{_baseAddress}/{query}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.ReadAsAsync<ApplicationUserDTO>();
        result.Should().BeEquivalentTo(expectation);
    }
    [Fact]
    public async Task GetByUserId_InvalidRequest_ReturnsNotFound()
    {
        // Arrange
        var query = Guid.NewGuid();
        
        // Act
        var response = await Client.GetAsync($"{_baseAddress}/{query}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}