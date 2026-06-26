using System.Security.Cryptography;
using System.Text;
using UrlShortener.Core.Contracts;

namespace UrlShortener.Infrastructure.Services;

public sealed class Base62KeyGenerator : IKeyGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public string Generate(string input, int length = 7)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        ArgumentNullException.ThrowIfNull(input);

        var buffer = new byte[length];
        _rng.GetBytes(buffer);

        var builder = new StringBuilder(length);
        foreach (byte b in buffer)
        {
            builder.Append(Alphabet[b % Alphabet.Length]);
        }

        return builder.ToString();
    }
}
