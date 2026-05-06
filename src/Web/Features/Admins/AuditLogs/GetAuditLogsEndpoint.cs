using System.Globalization;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NodaTime;
using NodaTime.Text;
using Web.Dtos;
using Web.Features.Common;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.AuditLogs;

public class GetAuditLogsRequest : PaginateRequest
{
    public string? User { get; set; }
    public string? ActionType { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
}

public class GetAuditLogsEndpoint : Endpoint<GetAuditLogsRequest, PaginatedList<AuditLogDto>>
{
    private static readonly DateTimeZone QuebecZone = DateTimeZoneProviders.Tzdb["America/Toronto"];
    private static readonly LocalDatePattern DatePattern = LocalDatePattern.CreateWithInvariantCulture("uuuu-MM-dd");

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public GetAuditLogsEndpoint(IAuditLogRepository auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/audit-logs");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAuditLogsRequest req, CancellationToken ct)
    {
        var fromInclusive = ParseStartOfDay(req.FromDate);
        var toExclusive = ParseStartOfNextDay(req.ToDate);

        var paginatedLogs = await _auditLogRepository.GetPaginated(
            req.NormalizedPageIndex,
            req.NormalizedPageSize,
            req.User,
            req.ActionType,
            fromInclusive,
            toExclusive,
            ct);

        await Send.OkAsync(
            new PaginatedList<AuditLogDto>(
                _mapper.Map<List<AuditLogDto>>(paginatedLogs.Items),
                paginatedLogs.TotalItems),
            cancellation: ct);
    }

    private static Instant? ParseStartOfDay(string? dateString)
    {
        var localDate = ParseLocalDate(dateString);
        return localDate?.AtStartOfDayInZone(QuebecZone).ToInstant();
    }

    private static Instant? ParseStartOfNextDay(string? dateString)
    {
        var localDate = ParseLocalDate(dateString);
        return localDate?.PlusDays(1).AtStartOfDayInZone(QuebecZone).ToInstant();
    }

    private static LocalDate? ParseLocalDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;
        var parseResult = DatePattern.Parse(dateString);
        return parseResult.Success ? parseResult.Value : null;
    }
}
