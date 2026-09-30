using System.Security.Cryptography;
using System.Text;

namespace NetCoreTemplate.Infrastructure.Security;

public static class HashHelper
{
    public static string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var builder = new StringBuilder();
        foreach (var b in bytes) builder.Append(b.ToString("x2"));
        return builder.ToString();
    }

    public static string Sha512(string input)
    {
        var bytes = SHA512.HashData(Encoding.UTF8.GetBytes(input));
        var builder = new StringBuilder();
        foreach (var b in bytes) builder.Append(b.ToString("x2"));
        return builder.ToString();
    }

    public static string HmacSha256(string input, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        var builder = new StringBuilder();
        foreach (var b in hashBytes) builder.Append(b.ToString("x2"));
        return builder.ToString();
    }

    public static string HmacSha512(string input, string key)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        var builder = new StringBuilder();
        foreach (var b in hashBytes) builder.Append(b.ToString("x2"));
        return builder.ToString();
    }
}
