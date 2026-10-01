using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Interfaces.Security;

public interface ITokenService
{
    (string accessToken, DateTime expiresAt) GenerateAccessToken(AppUser user);
    string GenerateRandomToken(int length = 32);
    DateTime GetRefreshTokenExpiration();
    Task<bool> ValidateTokenAsync(string token);
}
