using NodaTime;

namespace Domain.Entities;

public class BackupRecord
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; } = null!;
    public long SizeInBytes { get; private set; }
    public Instant CreatedAt { get; private set; }
    public string Type { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public string? ErrorMessage { get; private set; }

    public BackupRecord() { }

    public BackupRecord(string fileName, string type)
    {
        Id = Guid.NewGuid();
        FileName = fileName;
        Type = type;
        Status = BackupStatus.InProgress;
        CreatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public void MarkCompleted(long sizeInBytes)
    {
        Status = BackupStatus.Completed;
        SizeInBytes = sizeInBytes;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = BackupStatus.Failed;
        ErrorMessage = errorMessage;
    }
}

public static class BackupStatus
{
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

public static class BackupType
{
    public const string Manual = "Manual";
    public const string Scheduled = "Scheduled";
}
