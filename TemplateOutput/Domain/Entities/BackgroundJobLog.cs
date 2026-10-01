using $safeprojectname$.Domain.Entities.Seedworks;
using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Entities;

public class BackgroundJobLog : BaseEntity
{
    public string JobType { get; private set; } = null!;
    public JobStatus JobStatus { get; private set; }
    public string? Payload { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; } = 0;
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }

    private BackgroundJobLog() { }

    public static BackgroundJobLog Create(string jobType, string? payload = null)
    {
        return new BackgroundJobLog
        {
            JobType = jobType,
            Payload = payload,
            JobStatus = JobStatus.Pending,
            RetryCount = 0
        };
    }

    public void Start()
    {
        JobStatus = JobStatus.Processing;
        StartedAt = DateTime.UtcNow;
    }

    public void Succeed()
    {
        JobStatus = JobStatus.Succeeded;
        FinishedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        JobStatus = JobStatus.Failed;
        ErrorMessage = errorMessage;
        FinishedAt = DateTime.UtcNow;
    }

    public void ScheduleRetry(string errorMessage)
    {
        JobStatus = JobStatus.Retry;
        ErrorMessage = errorMessage;
        RetryCount++;
        FinishedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        JobStatus = JobStatus.Cancelled;
        FinishedAt = DateTime.UtcNow;
    }
}
