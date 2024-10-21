using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;
using School.Exam.SIUSS.Services.Authentication.Persistence;
using Serilog;
using Xunit;

namespace School.Exam.SIUSS.Services.Authentication.IntegrationTests.Setup;

[Collection("mysql")]
public class TestBase : IClassFixture<ApiWebApplicationFactory>, IDisposable
{
    protected HttpClient Client { get; }
    protected TestServer Server { get; }
    protected ApplicationUserContext Context { get; }
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly IServiceScope Scope;
    
    public TestBase(ApiWebApplicationFactory webApplicationFactory)
    {
        Client = webApplicationFactory.CreateClient();
        Server = webApplicationFactory.Server;
        Scope = webApplicationFactory.Services.CreateScope();
        
        Context = Scope.ServiceProvider.GetRequiredService<ApplicationUserContext>();
        UserManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        try
        {
            Context.Database.EnsureCreated();
        }
        catch
        {
            // Already exists
        }
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        
        Scope.Dispose();
        Log.CloseAndFlush();
        GC.SuppressFinalize(this);
    }
}