using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Services;
using UrlShortener.Infrastructure.Services;

namespace UrlShortener.Tests;

public class UrlShortenerServiceTests
{
    private readonly IUrlRepository _repository = new InMemoryUrlRepository();
    private readonly IClickEventRepository _clickEvents = new InMemoryClickEventRepository();
    private readonly IKeyGenerator _generator = new Base62KeyGenerator();
    private const string BaseUrl = "https://short.example/";

    private UrlShortenerService CreateService() =>
        new(_repository, _generator, _clickEvents, BaseUrl);

    [Fact]
    public async Task CreateShortUrlAsync_ShouldCreateShortUrl_WhenLongUrlIsNew()
    {
        var service = CreateService();
        var request = new CreateShortUrlRequest { LongUrl = "https://example.com/page" };

        var response = await service.CreateShortUrlAsync(request);

        Assert.NotNull(response);
        Assert.Equal("https://example.com/page", response.LongUrl);
        Assert.Equal(BaseUrl + response.ShortCode, response.ShortUrl);
        Assert.False(string.IsNullOrWhiteSpace(response.ShortCode));
    }

    [Fact]
    public async Task CreateShortUrlAsync_ShouldReturnExistingMapping_WhenLongUrlAlreadyExists()
    {
        var service = CreateService();
        var first = await service.CreateShortUrlAsync(new CreateShortUrlRequest { LongUrl = "https://example.com/page" });
        var second = await service.CreateShortUrlAsync(new CreateShortUrlRequest { LongUrl = "https://example.com/page" });

        Assert.Equal(first.ShortCode, second.ShortCode);
        Assert.Equal(first.ShortUrl, second.ShortUrl);
    }

    [Fact]
    public async Task ResolveAsync_ShouldReturnOriginalUrl_AndIncrementClickCount()
    {
        var service = CreateService();
        var response = await service.CreateShortUrlAsync(new CreateShortUrlRequest { LongUrl = "https://example.com/page" });

        var original = await service.ResolveAsync(response.ShortCode);
        var details = await service.GetDetailsAsync(response.ShortCode);

        Assert.Equal("https://example.com/page", original);
        Assert.Equal(1, details.ClickCount);
    }

    [Fact]
    public async Task GetAllUrlsAsync_ShouldReturnAllCreatedMappings()
    {
        var service = CreateService();
        await service.CreateShortUrlAsync(new CreateShortUrlRequest { LongUrl = "https://example.com/page1", UserId = "user1" });
        await service.CreateShortUrlAsync(new CreateShortUrlRequest { LongUrl = "https://example.com/page2", UserId = "user1" });

        var results = await service.GetAllUrlsAsync("user1");

        Assert.NotNull(results);
        Assert.Equal(2, results.Count());
    }
}
