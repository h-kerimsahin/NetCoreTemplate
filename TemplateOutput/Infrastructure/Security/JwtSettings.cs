namespace $safeprojectname$.Infrastructure.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "$safeprojectname$";
    public string Audience { get; set; } = "$safeprojectname$.Users";
    public string SecretKey { get; set; } = "THIS_IS_A_DEVELOPMENT_SECRET_KEY_PLEASE_CHANGE_IN_PRODUCTION_1234567890!@#$";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 14;
}
