using $safeprojectname$.Domain.Entities.Seedworks;
using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Entities;

public class SystemLog : BaseEntity
{
    public SystemLogLevel LogLevel { get; private set; }
    public string Message { get; private set; } = null!;
    public string? ExceptionType { get; private set; }
    public string? StackTrace { get; private set; }
    public string? Source { get; private set; }
    public string? RequestPath { get; private set; }
    public string? RequestMethod { get; private set; }
    public Guid? UserId { get; private set; }
    public string? IpAddress { get; private set; }

    private SystemLog() { }

    public SystemLog(SystemLogLevel logLevel, string message, string? exceptionType = null, string? stackTrace = null, string? source = null, string? requestPath = null, string? requestMethod = null, Guid? userId = null, string? ipAddress = null)
    {
        LogLevel = logLevel;
        Message = message;
        ExceptionType = exceptionType;
        StackTrace = stackTrace;
        Source = source;
        RequestPath = requestPath;
        RequestMethod = requestMethod;
        UserId = userId;
        IpAddress = ipAddress;
    }
}
