using System.Diagnostics;
using System.Formats.Tar;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Persistence;
using ZstdSharp;

namespace Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupPath;
    private readonly string _uploadsPath;
    private readonly int _retentionCount;

    public BackupService(
        GarneauTemplateDbContext context,
        IConfiguration configuration,
        ILogger<BackupService> logger,
        string webRootPath)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _backupPath = configuration["Backup:BackupPath"] ?? Path.Combine(webRootPath, "..", "backups");
        _uploadsPath = Path.Combine(webRootPath, "uploads");
        _retentionCount = int.TryParse(configuration["Backup:RetentionCount"], out var r) ? r : 7;
    }

    public async Task<BackupRecord> CreateBackupAsync(string type, CancellationToken ct)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
        var archiveFileName = $"backup-{timestamp}.tar.zst";
        var record = new BackupRecord(archiveFileName, type);

        _context.BackupRecords.Add(record);
        await _context.SaveChangesAsync(ct);

        Directory.CreateDirectory(_backupPath);

        try
        {
            var dumpPath = Path.Combine(_backupPath, $"temp-{record.Id}.dump");

            await BackupDatabaseAsync(dumpPath, ct);

            var archivePath = Path.Combine(_backupPath, archiveFileName);
            await CreateArchiveAsync(archivePath, dumpPath, ct);

            if (File.Exists(dumpPath))
                File.Delete(dumpPath);

            var fileInfo = new FileInfo(archivePath);
            record.MarkCompleted(fileInfo.Length);
            _context.BackupRecords.Update(record);
            await _context.SaveChangesAsync(ct);

            await ApplyRetentionPolicyAsync(ct);

            _logger.LogInformation("Backup {FileName} créé ({Size} octets)", archiveFileName, fileInfo.Length);
        }
        catch (Exception ex)
        {
            record.MarkFailed(ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message);
            _context.BackupRecords.Update(record);
            await _context.SaveChangesAsync(ct);

            _logger.LogError(ex, "Erreur lors de la création du backup {FileName}", archiveFileName);
            throw;
        }

        return record;
    }

    public async Task RestoreAsync(string fileName, CancellationToken ct)
    {
        ValidateFileName(fileName);
        var archivePath = Path.Combine(_backupPath, fileName);
        if (!File.Exists(archivePath))
            throw new FileNotFoundException($"Backup introuvable : {fileName}");

        var tempDir = Path.Combine(_backupPath, $"restore-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            await ExtractArchiveAsync(archivePath, tempDir, ct);

            var dumpPath = Path.Combine(tempDir, "database.dump");
            if (File.Exists(dumpPath))
            {
                await RestoreDatabaseAsync(dumpPath, ct);
            }

            var uploadsDir = Path.Combine(tempDir, "uploads");
            if (Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(_uploadsPath);
                foreach (var file in Directory.GetFiles(uploadsDir, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(uploadsDir, file);
                    var destPath = Path.Combine(_uploadsPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                    File.Copy(file, destPath, true);
                }
            }

            _logger.LogInformation("Restauration du backup {FileName} terminée", fileName);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var record = await _context.BackupRecords.FindAsync([id], ct);
        if (record is null) return;

        var filePath = Path.Combine(_backupPath, record.FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        _context.BackupRecords.Remove(record);
        await _context.SaveChangesAsync(ct);
    }

    public List<BackupRecord> GetAll()
    {
        return _context.BackupRecords
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public Task<Stream?> GetFileStreamAsync(string fileName, CancellationToken ct)
    {
        ValidateFileName(fileName);
        var path = Path.Combine(_backupPath, fileName);
        if (!File.Exists(path))
            return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(File.OpenRead(path));
    }

    public bool IsAvailable()
    {
        try
        {
            Directory.CreateDirectory(_backupPath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName != Path.GetFileName(fileName)
            || !fileName.EndsWith(".tar.zst", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Nom de fichier invalide.", nameof(fileName));
    }

    private async Task BackupDatabaseAsync(string dumpPath, CancellationToken ct)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        var psi = new ProcessStartInfo
        {
            FileName = "pg_dump",
            ArgumentList =
            {
                "-h", builder.Host ?? "localhost",
                "-p", (builder.Port > 0 ? builder.Port : 5432).ToString(),
                "-U", builder.Username ?? "postgres",
                "-d", builder.Database ?? "expressiondansebeauport",
                "-Fc",
                "-f", dumpPath
            },
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.Environment["PGPASSWORD"] = builder.Password ?? "";

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Impossible de démarrer pg_dump.");

        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"pg_dump a échoué (code {process.ExitCode}) : {stderr}");
    }

    private async Task RestoreDatabaseAsync(string dumpPath, CancellationToken ct)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        var psi = new ProcessStartInfo
        {
            FileName = "pg_restore",
            ArgumentList =
            {
                "-h", builder.Host ?? "localhost",
                "-p", (builder.Port > 0 ? builder.Port : 5432).ToString(),
                "-U", builder.Username ?? "postgres",
                "-d", builder.Database ?? "expressiondansebeauport",
                "--clean",
                "--if-exists",
                "--no-owner",
                "--single-transaction",
                dumpPath
            },
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.Environment["PGPASSWORD"] = builder.Password ?? "";

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Impossible de démarrer pg_restore.");

        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
            _logger.LogWarning("pg_restore terminé avec code {ExitCode} : {Stderr}", process.ExitCode, stderr);
    }

    private async Task CreateArchiveAsync(string archivePath, string dumpPath, CancellationToken ct)
    {
        await using var fileStream = File.Create(archivePath);
        await using var zstdStream = new CompressionStream(fileStream, 3);
        await using var tarWriter = new TarWriter(zstdStream);

        if (File.Exists(dumpPath))
        {
            await tarWriter.WriteEntryAsync(dumpPath, "database.dump", ct);
        }

        if (Directory.Exists(_uploadsPath))
        {
            foreach (var file in Directory.GetFiles(_uploadsPath, "*", SearchOption.AllDirectories))
            {
                var entryName = "uploads/" + Path.GetRelativePath(_uploadsPath, file);
                await tarWriter.WriteEntryAsync(file, entryName, ct);
            }
        }
    }

    private static async Task ExtractArchiveAsync(string archivePath, string destDir, CancellationToken ct)
    {
        await using var fileStream = File.OpenRead(archivePath);
        await using var zstdStream = new DecompressionStream(fileStream);
        await TarFile.ExtractToDirectoryAsync(zstdStream, destDir, true, ct);
    }

    private async Task ApplyRetentionPolicyAsync(CancellationToken ct)
    {
        var allRecords = await _context.BackupRecords
            .Where(x => x.Status == BackupStatus.Completed)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        if (allRecords.Count <= _retentionCount) return;

        var toDelete = allRecords.Skip(_retentionCount).ToList();
        foreach (var record in toDelete)
        {
            var filePath = Path.Combine(_backupPath, record.FileName);
            if (File.Exists(filePath))
                File.Delete(filePath);

            _context.BackupRecords.Remove(record);
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("{Count} ancien(s) backup(s) supprimé(s) par la politique de rétention", toDelete.Count);
    }
}
