using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UrlShortener.Core.Contracts;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("")]
public sealed class RedirectController : ControllerBase
{
    private readonly IUrlShortenerService _service;

    public RedirectController(IUrlShortenerService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpGet("{shortCode}")]
    [EnableRateLimiting("ReadTrafficPolicy")]
    public async Task<IActionResult> RedirectToOriginalAsync(string shortCode)
    {
        try
        {
            var referrer = Request.Headers.Referer.ToString();
            var country = Request.Headers["CF-IPCountry"].FirstOrDefault()
                ?? Request.Headers["X-Country-Code"].FirstOrDefault()
                ?? "India";
            var originalUrl = await _service.ResolveAsync(shortCode, referrer, country);
            return Redirect(originalUrl);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
