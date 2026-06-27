using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UrlController : ControllerBase
{
    private readonly IUrlShortenerService _service;

    public UrlController(IUrlShortenerService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost("shorten")]
    [EnableRateLimiting("WriteTrafficPolicy")]
    public async Task<IActionResult> CreateShortUrlAsync([FromBody] CreateShortUrlRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var response = await _service.CreateShortUrlAsync(request);
        return CreatedAtRoute("GetUrlDetails", new { shortCode = response.ShortCode }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] string? userId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 0)
    {
        if (page > 0 || pageSize > 0 || !string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(status))
        {
            var paged = await _service.GetPagedUrlsAsync(userId, search, status, Math.Max(1, page), pageSize > 0 ? pageSize : 10);
            return Ok(paged);
        }

        var results = await _service.GetAllUrlsAsync(userId);
        return Ok(results);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentAsync([FromQuery] string? userId = null, [FromQuery] int limit = 5)
    {
        var results = await _service.GetRecentUrlsAsync(userId, limit);
        return Ok(results);
    }

    [HttpGet("{shortCode}", Name = "GetUrlDetails")]
    public async Task<IActionResult> GetDetailsAsync(string shortCode)
    {
        var details = await _service.GetDetailsAsync(shortCode);
        return Ok(details);
    }

    [HttpDelete("{shortCode}")]
    public async Task<IActionResult> DeleteAsync(string shortCode, [FromQuery] string? userId = null)
    {
        await _service.DeleteUrlAsync(shortCode, userId);
        return NoContent();
    }

    [HttpPost("{shortCode}/qr-scan")]
    public async Task<IActionResult> RecordQrScanAsync(string shortCode)
    {
        await _service.RecordQrScanAsync(shortCode);
        return NoContent();
    }

    [HttpGet("analytics/{shortCode}")]
    public async Task<IActionResult> GetAnalyticsAsync(string shortCode)
    {
        var analytics = await _service.GetAnalyticsAsync(shortCode);
        return Ok(analytics);
    }
}
