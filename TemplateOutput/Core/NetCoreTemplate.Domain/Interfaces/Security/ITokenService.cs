using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Interfaces.Security;

public interface ITokenService
{
    (string accessToken, DateTime expiresAt) GenerateAccessToken(AppUser user);
    string GenerateRandomToken(int length = 32);
    DateTime GetRefreshTokenExpiration();
    Task<bool> ValidateTokenAsync(string token);
}
