using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.Contracts;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/workspace")]
public sealed class WorkspaceController : ControllerBase
{
    private readonly IUserService _userService;

    public WorkspaceController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpGet("keys")]
    public async Task<IActionResult> GetApiKeyAsync([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId is required.");
        }

        var profile = await _userService.GetProfileAsync(userId);
        return Ok(new { maskedApiKey = profile.MaskedApiKey });
    }

    [HttpPost("keys/regenerate")]
    public async Task<IActionResult> RegenerateApiKeyAsync([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId is required.");
        }

        var response = await _userService.RegenerateApiKeyAsync(userId);
        return Ok(response);
    }
}
