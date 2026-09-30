using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool VerifyPassword(string password, string passwordHash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, passwordHash); }
        catch { return false; }
    }

    public bool NeedsRehash(string passwordHash)
    {
        try { return BCrypt.Net.BCrypt.PasswordNeedsRehash(passwordHash, WorkFactor); }
        catch { return false; }
    }
}
