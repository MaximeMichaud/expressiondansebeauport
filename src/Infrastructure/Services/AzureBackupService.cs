using Application.Interfaces.Services;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using Azure.Storage.Blobs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Services;

public class AzureBackupService : IBackupService
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureBackupService> _logger;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly int _retentionCount;

    public AzureBackupService(
        GarneauTemplateDbContext context,
        IConfiguration configuration,
        ILogger<AzureBackupService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _retentionCount = int.TryParse(configuration["Backup:RetentionCount"], out var r) ? r : 7;

        var storageConnectionString = configuration["Azure:StorageConnectionString"]
            ?? throw new InvalidOperationException("Azure:StorageConnectionString est requis pour le provider Azure.");
        var containerName = configuration["Azure:BlobContainerName"] ?? "backups";
        _blobContainerClient = new BlobContainerClient(storageConnectionString, containerName);
    }

    public async Task<BackupRecord> CreateBackupAsync(string type, CancellationToken ct)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
        var fileName = $"backup-{timestamp}.bacpac";
        var record = new BackupRecord(fileName, type);

        _context.BackupRecords.Add(record);
        await _context.SaveChangesAsync(ct);

        try
        {
            await _blobContainerClient.CreateIfNotExistsAsync(cancellationToken: ct);

            var subscriptionId = _configuration["Azure:SubscriptionId"]
                ?? throw new InvalidOperationException("Azure:SubscriptionId est requis.");
            var resourceGroup = _configuration["Azure:ResourceGroupName"]
                ?? throw new InvalidOperationException("Azure:ResourceGroupName est requis.");
            var sqlServerName = _configuration["Azure:SqlServerName"]
                ?? throw new InvalidOperationException("Azure:SqlServerName est requis.");
            var sqlAdminUser = _configuration["Azure:SqlAdminUser"]
                ?? throw new InvalidOperationException("Azure:SqlAdminUser est requis.");
            var sqlAdminPassword = _configuration["Azure:SqlAdminPassword"]
                ?? throw new InvalidOperationException("Azure:SqlAdminPassword est requis.");
            var storageKey = _configuration["Azure:StorageAccountKey"]
                ?? throw new InvalidOperationException("Azure:StorageAccountKey est requis.");

            var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            var databaseName = _configuration["Azure:SqlDatabaseName"] ?? builder.InitialCatalog;

            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            var storageUri = blobClient.Uri;

            var armClient = new ArmClient(new DefaultAzureCredential());
            var databaseResourceId = SqlDatabaseResource.CreateResourceIdentifier(
                subscriptionId, resourceGroup, sqlServerName, databaseName);
            var database = armClient.GetSqlDatabaseResource(databaseResourceId);

            var exportData = new DatabaseExportDefinition(
                storageKeyType: StorageKeyType.StorageAccessKey,
                storageKey: storageKey,
                storageUri: storageUri,
                administratorLogin: sqlAdminUser,
                administratorLoginPassword: sqlAdminPassword);

            _logger.LogInformation("Lancement de l'export BACPAC {FileName} vers Azure Blob Storage", fileName);
            var operation = await database.ExportAsync(Azure.WaitUntil.Completed, exportData, ct);

            var blobProperties = await blobClient.GetPropertiesAsync(cancellationToken: ct);
            var size = blobProperties.Value.ContentLength;

            record.MarkCompleted(size);
            _context.BackupRecords.Update(record);
            await _context.SaveChangesAsync(ct);

            await ApplyRetentionPolicyAsync(ct);

            _logger.LogInformation("Backup Azure {FileName} terminé ({Size} octets)", fileName, size);
        }
        catch (Exception ex)
        {
            record.MarkFailed(ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message);
            _context.BackupRecords.Update(record);
            await _context.SaveChangesAsync(ct);

            _logger.LogError(ex, "Erreur lors de la création du backup Azure {FileName}", fileName);
            throw;
        }

        return record;
    }

    public async Task RestoreAsync(string fileName, CancellationToken ct)
    {
        ValidateFileName(fileName);

        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync(ct))
            throw new FileNotFoundException($"Backup introuvable dans le Blob Storage : {fileName}");

        var subscriptionId = _configuration["Azure:SubscriptionId"]!;
        var resourceGroup = _configuration["Azure:ResourceGroupName"]!;
        var sqlServerName = _configuration["Azure:SqlServerName"]!;
        var sqlAdminUser = _configuration["Azure:SqlAdminUser"]!;
        var sqlAdminPassword = _configuration["Azure:SqlAdminPassword"]!;
        var storageKey = _configuration["Azure:StorageAccountKey"]!;

        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        var databaseName = _configuration["Azure:SqlDatabaseName"] ?? builder.InitialCatalog;

        var armClient = new ArmClient(new DefaultAzureCredential());
        var databaseResourceId = SqlDatabaseResource.CreateResourceIdentifier(
            subscriptionId, resourceGroup, sqlServerName, databaseName);
        var database = armClient.GetSqlDatabaseResource(databaseResourceId);

        var importData = new ImportExistingDatabaseDefinition(
            storageKeyType: StorageKeyType.StorageAccessKey,
            storageKey: storageKey,
            storageUri: blobClient.Uri,
            administratorLogin: sqlAdminUser,
            administratorLoginPassword: sqlAdminPassword);

        _logger.LogInformation("Import BACPAC {FileName} vers la base {Database}", fileName, databaseName);
        await database.ImportAsync(Azure.WaitUntil.Completed, importData, ct);

        _logger.LogInformation("Restauration Azure du backup {FileName} terminée", fileName);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var record = await _context.BackupRecords.FindAsync([id], ct);
        if (record is null) return;

        var blobClient = _blobContainerClient.GetBlobClient(record.FileName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);

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

    public async Task<Stream?> GetFileStreamAsync(string fileName, CancellationToken ct)
    {
        ValidateFileName(fileName);
        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync(ct))
            return null;
        return await blobClient.OpenReadAsync(cancellationToken: ct);
    }

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName != Path.GetFileName(fileName)
            || !fileName.EndsWith(".bacpac", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Nom de fichier invalide.", nameof(fileName));
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
            var blobClient = _blobContainerClient.GetBlobClient(record.FileName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
            _context.BackupRecords.Remove(record);
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("{Count} ancien(s) backup(s) supprimé(s) par la politique de rétention", toDelete.Count);
    }
}
