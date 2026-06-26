using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync([FromQuery] string? userId = null)
    {
        var stats = await _dashboardService.GetStatsAsync(userId);
        return Ok(stats);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync([FromQuery] string? userId = null)
    {
        var summary = await _dashboardService.GetSummaryAsync(userId);
        return Ok(summary);
    }

    [HttpGet("velocity")]
    public async Task<IActionResult> GetVelocityAsync([FromQuery] string? userId = null)
    {
        var velocity = await _dashboardService.GetVelocityAsync(userId);
        return Ok(velocity);
    }
}
