using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.VisualBasic;
using School.Exam.SIUSS.Services.Authentication.Models.DTOs;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;
using School.Exam.SIUSS.Services.Authentication.Models.Requests;
using School.Shared.Core.Abstractions.Exceptions;
using School.Shared.Core.Authentication.Claims;

namespace School.Exam.SIUSS.Services.Authentication.Services;

public interface IIdentityService
{
    public Task<ApplicationUserDTO> Create(CreateIdentityRequest request);
    public Task<ApplicationUserDTO?> GetByUserId(Guid userId);
    public Task<ApplicationUser> Authorize(CreateUserSessionRequest request);
}

public class IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<IdentityService> logger)
    : IIdentityService
{
    public async Task<ApplicationUserDTO> Create(CreateIdentityRequest request)
    {
        var validator = new MinLengthAttribute(6);
        if (string.IsNullOrEmpty(request.UserName) || !validator.IsValid(request.UserName))
        {
            throw new BadRequestException("Invalid Username");
        }

        var entity = new ApplicationUser
        {
            UserName = request.UserName,
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        var result = await userManager.CreateAsync(entity, request.Password);

        string errorReason;

        if (!result.Succeeded)
        {
            errorReason = result.Errors.First().Description;
            logger.LogError("Failed to create ApplicationUser with username: {username} because: {reason}", entity.UserName, errorReason);
            throw new BadRequestException($"Failed to create ApplicationUser with username: {entity.UserName} because: {errorReason}");
        }

        result = await AddDefaultClaims(entity);

        if (!result.Succeeded)
        {
            errorReason = result.Errors.First().Description;
            logger.LogError("Failed while giving default claims to username: {username} because: {reason}", entity.UserName, errorReason);
            await userManager.DeleteAsync(entity);
            throw new BadRequestException($"Failed while giving default claims to username: {entity.UserName} because: {errorReason}");
        }
        
        logger.LogInformation("Successfully created ApplicationUser for {username} with id: {id} and userId {userId}", entity.UserName, entity.Id, entity.UserId);

        var dto = entity.Adapt<ApplicationUserDTO>();

        return dto;
    }

    public async Task<ApplicationUserDTO?> GetByUserId(Guid userId)
    {
        var entity = await userManager.Users.FirstOrDefaultAsync(x => x.UserId == userId);

        if (entity is null)
        {
            logger.LogError("Failed to retrieve user with userId {userId}", userId);
            throw new NotFoundException($"Failed to retrieve user with userId {userId}");
        }

        var dto = entity.Adapt<ApplicationUserDTO>();

        return dto;
    }

    public async Task<ApplicationUser> Authorize(CreateUserSessionRequest request)
    {
        var entity = await userManager.FindByNameAsync(request.UserName);
        
        if (entity is null)
        {
            logger.LogError("Failed to find user: {username}", request.UserName);
            throw new NotFoundException($"Failed to find user with username: {request.UserName}");
        }

        var signin = await signInManager.CheckPasswordSignInAsync(entity, request.Password, false);
        
        // TODO: Customize requirements .. Such as Email confirmation, 2FA option etc

        if (signin.IsLockedOut)
        {
            logger.LogError("{username} is locked out!", request.UserName);
            throw new BadRequestException($"{request.UserName} is locked out!");
        }

        if (!signin.Succeeded)
        {
            throw new BadRequestException($"Login failed");
        }

        return entity;
    }

    private async Task<IdentityResult> AddDefaultClaims(ApplicationUser entity)
    {
        var claims = new List<Claim>
        {
            new("iid", entity.Id.ToString()),
            new("uid", entity.UserId.ToString()),
            new(JwtRegisteredClaimNames.Sub, entity.UserName!),
            new("systems:website:role", Enum.GetName(ClaimLevel.User)!),
            new("systems:game:role", Enum.GetName(ClaimLevel.User)!)
        };

        return await userManager.AddClaimsAsync(entity, claims);
    }
}