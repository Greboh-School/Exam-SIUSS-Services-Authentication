using Microsoft.AspNetCore.Mvc;
using School.Exam.SIUSS.Services.Authentication.Models.DTOs;
using School.Exam.SIUSS.Services.Authentication.Models.Requests;
using School.Exam.SIUSS.Services.Authentication.Services;

namespace School.Exam.SIUSS.Services.Authentication.Controllers;

// TODO: I generally dont like mixing responsibilities across services / controllers. ex: DI IIdentityService & ITokenService .. Should look into facade pattern
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class SessionsController(IIdentityService identityService, ITokenService tokenService)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserSessionDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSessionDTO>> Create([FromBody] CreateUserSessionRequest request)
    {
        var entity = await identityService.Authorize(request);
        var accessToken = await tokenService.Create(entity);
        
        var dto = new UserSessionDTO(UserId: entity.UserId, UserName: entity.UserName!, AccessToken: accessToken);

        return Ok(dto);
    }
}