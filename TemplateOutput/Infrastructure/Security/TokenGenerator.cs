using System.Security.Cryptography;

namespace $safeprojectname$.Infrastructure.Security;

public static class TokenGenerator
{
    public static string GenerateSecureToken(int byteLength = 32) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteLength));

    public static string GenerateNumericCode(int length = 6)
    {
        var code = new char[length];
        var bytes = RandomNumberGenerator.GetBytes(length);
        for (var i = 0; i < length; i++) code[i] = (char)('0' + bytes[i] % 10);
        return new string(code);
    }

    public static Guid GenerateGuid() => Guid.NewGuid();
}
