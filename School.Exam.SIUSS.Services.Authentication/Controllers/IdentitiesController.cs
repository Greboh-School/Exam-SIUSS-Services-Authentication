using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School.Exam.SIUSS.Services.Authentication.Models.DTOs;
using School.Exam.SIUSS.Services.Authentication.Models.Requests;
using School.Exam.SIUSS.Services.Authentication.Services;

namespace School.Exam.SIUSS.Services.Authentication.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class IdentitiesController(IIdentityService identityService, ILogger<IdentitiesController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApplicationUserDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApplicationUserDTO>> Create([FromBody] CreateIdentityRequest request)
    {
        var result = await identityService.Create(request);

        return CreatedAtAction(nameof(Get), new { userId = result.UserId }, result);
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplicationUserDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize("game:admin")]
    public async Task<ActionResult<ApplicationUserDTO>> Get([FromRoute] Guid userId)
    {
        var result = await identityService.GetByUserId(userId);

        return Ok(result);
    }
}