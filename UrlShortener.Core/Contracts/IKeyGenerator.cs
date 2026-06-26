namespace UrlShortener.Core.Contracts;

public interface IKeyGenerator
{
    string Generate(string input, int length = 7);
}
