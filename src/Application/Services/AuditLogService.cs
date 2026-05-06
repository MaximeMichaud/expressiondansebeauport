using System.Text.RegularExpressions;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Users;
using Domain.Entities;
using Domain.Helpers;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services;

public partial class AuditLogService : IAuditLogService
{
    private readonly IAdministratorRepository _administratorRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        IAuthenticatedUserService authenticatedUserService,
        IAdministratorRepository administratorRepository,
        ISiteSettingsRepository siteSettingsRepository,
        IUserRepository userRepository,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _authenticatedUserService = authenticatedUserService;
        _administratorRepository = administratorRepository;
        _siteSettingsRepository = siteSettingsRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task LogAsync(
        string actionType,
        string entityType,
        Guid? entityId = null,
        string? details = null,
        Guid? userId = null,
        string? userDisplayName = null,
        string? userEmail = null)
    {
        try
        {
            var actor = ResolveActor(userId, userDisplayName, userEmail);
            var auditLog = new AuditLog(
                actor.UserId,
                actor.UserDisplayName,
                actor.UserEmail,
                actionType,
                entityType,
                entityId,
                SanitizeDetails(details),
                InstantHelper.GetLocalNow());

            await _auditLogRepository.Create(auditLog);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Unable to persist audit log for {ActionType}/{EntityType}", actionType, entityType);
        }
    }

    public async Task PurgeExpiredLogsAsync()
    {
        var settings = await _siteSettingsRepository.Get();
        var retentionDays = Math.Max(settings.AuditLogRetentionDays, 1);
        var cutoff = InstantHelper.GetLocalNow().Minus(Duration.FromDays(retentionDays));
        await _auditLogRepository.DeleteOlderThan(cutoff);
    }

    private (Guid? UserId, string? UserDisplayName, string? UserEmail) ResolveActor(
        Guid? explicitUserId,
        string? explicitUserDisplayName,
        string? explicitUserEmail)
    {
        if (explicitUserId.HasValue || !string.IsNullOrWhiteSpace(explicitUserDisplayName) || !string.IsNullOrWhiteSpace(explicitUserEmail))
        {
            if (explicitUserId.HasValue)
            {
                var admin = _administratorRepository.FindByUserId(explicitUserId.Value);
                if (admin is not null)
                {
                    return (explicitUserId, explicitUserDisplayName ?? admin.FullName, explicitUserEmail ?? admin.Email);
                }

                var user = _userRepository.FindById(explicitUserId.Value);
                if (user is not null)
                {
                    return (explicitUserId, explicitUserDisplayName ?? user.Email, explicitUserEmail ?? user.Email);
                }
            }

            return (explicitUserId, explicitUserDisplayName, explicitUserEmail);
        }

        var authenticatedUser = _authenticatedUserService.GetAuthenticatedUser();
        if (authenticatedUser is null)
            return (null, null, null);

        var authenticatedAdmin = _administratorRepository.FindByUserId(authenticatedUser.Id);
        return (
            authenticatedUser.Id,
            authenticatedAdmin?.FullName ?? authenticatedUser.Email,
            authenticatedUser.Email);
    }

    private static string? SanitizeDetails(string? details)
    {
        if (string.IsNullOrWhiteSpace(details))
            return details;

        var sanitized = SensitivePairRegex().Replace(details, static match => $"{match.Groups["key"].Value}=***");
        sanitized = SensitiveColonRegex().Replace(sanitized, static match => $"{match.Groups["key"].Value}: ***");
        return sanitized;
    }

    [GeneratedRegex(@"(?<key>password|token|secret|api[-_ ]?key|refresh[-_ ]?token|access[-_ ]?token)\s*=\s*[^,;\r\n]+", RegexOptions.IgnoreCase)]
    private static partial Regex SensitivePairRegex();

    [GeneratedRegex(@"(?<key>password|token|secret|api[-_ ]?key|refresh[-_ ]?token|access[-_ ]?token)\s*:\s*[^,;\r\n]+", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveColonRegex();
}
