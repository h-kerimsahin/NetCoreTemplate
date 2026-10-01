namespace $safeprojectname$.Infrastructure.Idempotency;

public class IdempotencySettings
{
    public int SlidingExpirationMinutes { get; set; } = 120;
}
