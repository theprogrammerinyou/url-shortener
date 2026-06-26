using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.Contracts;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> GetLinkAnalyticsAsync(string shortCode)
    {
        var analytics = await _analyticsService.GetLinkAnalyticsAsync(shortCode);
        return Ok(analytics);
    }

    [HttpGet("{shortCode}/clicks")]
    public async Task<IActionResult> GetClicksOverTimeAsync(string shortCode, [FromQuery] string period = "daily")
    {
        var clicks = await _analyticsService.GetClicksOverTimeAsync(shortCode, period);
        return Ok(clicks);
    }

    [HttpGet("{shortCode}/referrers")]
    public async Task<IActionResult> GetReferrersAsync(string shortCode)
    {
        var referrers = await _analyticsService.GetReferrersAsync(shortCode);
        return Ok(referrers);
    }

    [HttpGet("{shortCode}/geo")]
    public async Task<IActionResult> GetGeoDistributionAsync(string shortCode)
    {
        var geo = await _analyticsService.GetGeoDistributionAsync(shortCode);
        return Ok(geo);
    }
}
