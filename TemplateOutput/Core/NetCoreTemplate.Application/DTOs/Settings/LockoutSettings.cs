namespace NetCoreTemplate.Application.DTOs.Settings;

public record LockoutSettings(
    int MaxFailedAttempts = 5,
    int LockoutMinutes = 15,
    int ResetFailedCountAfterMinutes = 30
);
