using UrlShortener.Core.Entities;
using UrlShortener.Infrastructure.Services;

namespace UrlShortener.Tests;

public class InMemoryUrlRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldStoreUrl()
    {
        var repository = new InMemoryUrlRepository();
        var urlEntry = new UrlEntry("abc1234", "https://example.com", DateTime.UtcNow.AddDays(30));

        await repository.AddAsync(urlEntry);
        var result = await repository.FindByShortCodeAsync("abc1234");

        Assert.NotNull(result);
        Assert.Equal(urlEntry.OriginalUrl, result!.OriginalUrl);
    }

    [Fact]
    public async Task FindByOriginalUrlAsync_ShouldReturnUrl()
    {
        var repository = new InMemoryUrlRepository();
        var urlEntry = new UrlEntry("abc1234", "https://example.com", DateTime.UtcNow.AddDays(30));

        await repository.AddAsync(urlEntry);
        var result = await repository.FindByOriginalUrlAsync("https://example.com");

        Assert.NotNull(result);
        Assert.Equal("abc1234", result!.ShortCode);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUrls()
    {
        var repository = new InMemoryUrlRepository();

        await repository.AddAsync(new UrlEntry("abc1234", "https://example.com/page1", DateTime.UtcNow.AddDays(30)));
        await repository.AddAsync(new UrlEntry("def5678", "https://example.com/page2", DateTime.UtcNow.AddDays(30)));

        var results = await repository.GetAllAsync();

        Assert.Equal(2, results.Count());
    }
}
