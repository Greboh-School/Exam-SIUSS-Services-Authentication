using Microsoft.AspNetCore.Identity;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;
using School.Exam.SIUSS.Services.Authentication.Persistence;
using School.Exam.SIUSS.Services.Authentication.Services;
using School.Shared.Core.Abstractions;
using School.Shared.Core.Persistence.Extensions;

namespace School.Exam.SIUSS.Services.Authentication;

public class Configuration : ServiceConfiguration
{
    public override void InjectMiddleware(IApplicationBuilder builder)
    {
        
    }

    public override void InjectServiceRegistrations(IServiceCollection services)
    {
        // Persistence
        services.AddMySQLContext<ApplicationUserContext>("users", Configuration);
        
        // Options
        services.Configure<Models.Options.TokenOptions>(Configuration.GetSection(Models.Options.TokenOptions.Section)); 
        
        // Services
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationUserContext>()
            .AddDefaultTokenProviders();
        
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<ITokenService, TokenService>();
    }
}