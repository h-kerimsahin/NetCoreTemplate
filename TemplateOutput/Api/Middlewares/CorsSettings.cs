namespace $safeprojectname$.Api.Middlewares;

public static class CorsSettings
{
    public static string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public static bool AllowAllInDevelopment { get; set; } = true;
    public static bool AllowCredentials { get; set; } = false;
    public static string[] ExposedHeaders { get; set; } = { "X-Pagination", "X-Idempotency-Key", "X-Total-Count" };
}
