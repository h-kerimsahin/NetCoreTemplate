namespace $safeprojectname$.Infrastructure.Services;

public class EmailSettings
{
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SmtpUserName { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public string FromName { get; set; } = "$safeprojectname$";
    public string FromEmail { get; set; } = "noreply@$safeprojectname$.local";
    public string AppBaseUrl { get; set; } = "https://localhost:7000";
}
