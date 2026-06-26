using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/user")]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfileAsync([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId is required.");
        }

        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateUserProfileRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest("userId is required.");
        }

        var profile = await _userService.UpdateProfileAsync(request);
        return Ok(profile);
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferencesAsync([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId is required.");
        }

        var preferences = await _userService.GetPreferencesAsync(userId);
        return Ok(preferences);
    }

    [HttpPatch("preferences")]
    public async Task<IActionResult> UpdatePreferencesAsync([FromBody] UpdateUserPreferencesRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest("userId is required.");
        }

        var preferences = await _userService.UpdatePreferencesAsync(request);
        return Ok(preferences);
    }
}

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
