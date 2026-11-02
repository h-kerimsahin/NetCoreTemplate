using NetCoreTemplate.Application.DTOs.Settings;

namespace NetCoreTemplate.Application.DTOs.Common;

public enum PasswordStrength
{
    TooShort = 1,
    Weak = 2,
    Medium = 3,
    Strong = 4,
    VeryStrong = 5
}

public static class PasswordStrengthCalculator
{
    public static PasswordStrength Calculate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordStrength.TooShort;

        int score = 0;

        if (password.Length >= 12)
            score++;

        if (password.Any(char.IsUpper))
            score++;

        if (password.Any(char.IsLower))
            score++;

        if (password.Any(char.IsDigit))
            score++;

        if (password.Any(ch => !char.IsLetterOrDigit(ch)))
            score++;

        if (password.Distinct().Count() >= 8)
            score++;

        return score switch
        {
            < 2 => PasswordStrength.TooShort,
            2 => PasswordStrength.Weak,
            3 => PasswordStrength.Medium,
            4 => PasswordStrength.Strong,
            >= 5 => PasswordStrength.VeryStrong
        };
    }

    public static bool IsStrongPassword(string password, PasswordSettings settings)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (password.Length < settings.MinLength)
            return false;

        if (settings.RequireUppercase && !password.Any(char.IsUpper))
            return false;

        if (settings.RequireLowercase && !password.Any(char.IsLower))
            return false;

        if (settings.RequireDigit && !password.Any(char.IsDigit))
            return false;

        if (settings.RequireSpecialChar)
        {
            var specialChars = settings.SpecialChars.ToHashSet();
            if (!password.Any(ch => specialChars.Contains(ch)))
                return false;
        }

        return true;
    }
}
