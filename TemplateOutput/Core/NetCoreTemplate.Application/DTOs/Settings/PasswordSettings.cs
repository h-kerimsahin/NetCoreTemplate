namespace NetCoreTemplate.Application.DTOs.Settings;

public record PasswordSettings(
    int MinLength = 8,
    bool RequireUppercase = true,
    bool RequireLowercase = true,
    bool RequireDigit = true,
    bool RequireSpecialChar = true,
    string SpecialChars = @"!@#$%^&*()_+\-=\[\]{};':\\|,.<>\/?"
);
