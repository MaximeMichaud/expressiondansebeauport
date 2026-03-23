using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IBackupService
{
    Task<BackupRecord> CreateBackupAsync(string type, CancellationToken ct);
    Task RestoreAsync(string fileName, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    List<BackupRecord> GetAll();
    string? GetFilePath(string fileName);
}
