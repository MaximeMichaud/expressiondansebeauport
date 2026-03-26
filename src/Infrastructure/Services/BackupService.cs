using System.Formats.Tar;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence;
using ZstdSharp;

namespace Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupPath;
    private readonly string _databaseBackupPath;
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
        _databaseBackupPath = configuration["Backup:DatabaseBackupPath"] ?? _backupPath;
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
            var tempBakName = $"temp-{record.Id}.bak";
            var sqlBackupPath = Path.Combine(_databaseBackupPath, tempBakName);
            var localBakPath = Path.Combine(_backupPath, tempBakName);

            await BackupDatabaseAsync(sqlBackupPath, ct);

            var archivePath = Path.Combine(_backupPath, archiveFileName);
            await CreateArchiveAsync(archivePath, localBakPath, ct);

            if (File.Exists(localBakPath))
                File.Delete(localBakPath);

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

            var bakPath = Path.Combine(tempDir, "database.bak");
            if (File.Exists(bakPath))
            {
                var restoreId = Guid.NewGuid();
                var localRestorePath = Path.Combine(_backupPath, $"restore-{restoreId}.bak");
                var sqlRestorePath = Path.Combine(_databaseBackupPath, $"restore-{restoreId}.bak");
                File.Copy(bakPath, localRestorePath, true);
                if (!OperatingSystem.IsWindows())
                    File.SetUnixFileMode(localRestorePath, UnixFileMode.OtherRead | UnixFileMode.GroupRead | UnixFileMode.UserRead | UnixFileMode.UserWrite);

                try
                {
                    await RestoreDatabaseAsync(sqlRestorePath, ct);
                }
                finally
                {
                    if (File.Exists(localRestorePath))
                        File.Delete(localRestorePath);
                }
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

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName != Path.GetFileName(fileName)
            || !fileName.EndsWith(".tar.zst", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Nom de fichier invalide.", nameof(fileName));
    }

    private async Task BackupDatabaseAsync(string backupPath, CancellationToken ct)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        var sql = $"""
            BACKUP DATABASE [{databaseName}]
            TO DISK = N'{backupPath}'
            WITH FORMAT, INIT, COMPRESSION,
                 CHECKSUM, STATS = 10
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = 600;
        await command.ExecuteNonQueryAsync(ct);
    }

    private async Task RestoreDatabaseAsync(string backupPath, CancellationToken ct)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        var masterConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync(ct);

        var setSingleUser = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
        await using (var cmd = new SqlCommand(setSingleUser, connection) { CommandTimeout = 60 })
            await cmd.ExecuteNonQueryAsync(ct);

        try
        {
            var restoreSql = $"""
                RESTORE DATABASE [{databaseName}]
                FROM DISK = N'{backupPath}'
                WITH REPLACE, STATS = 10;
                """;
            await using var cmd = new SqlCommand(restoreSql, connection) { CommandTimeout = 900 };
            await cmd.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            var setMultiUser = $"ALTER DATABASE [{databaseName}] SET MULTI_USER;";
            await using var cmd = new SqlCommand(setMultiUser, connection) { CommandTimeout = 60 };
            await cmd.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

    private async Task CreateArchiveAsync(string archivePath, string bakPath, CancellationToken ct)
    {
        await using var fileStream = File.Create(archivePath);
        await using var zstdStream = new CompressionStream(fileStream, 3);
        await using var tarWriter = new TarWriter(zstdStream);

        if (File.Exists(bakPath))
        {
            await tarWriter.WriteEntryAsync(bakPath, "database.bak", ct);
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
