using Domain.Common;
using Domain.Helpers;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NodaTime;
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
        var fromInclusive = InstantHelper.ParseFromNullableString(req.FromDate);
        var toExclusive = BuildToExclusive(req.ToDate);

        var paginatedLogs = _auditLogRepository.GetPaginated(
            req.PageIndex,
            req.PageSize,
            req.User,
            req.ActionType,
            fromInclusive,
            toExclusive);

        await Send.OkAsync(
            new PaginatedList<AuditLogDto>(
                _mapper.Map<List<AuditLogDto>>(paginatedLogs.Items),
                paginatedLogs.TotalItems),
            cancellation: ct);
    }

    private static Instant? BuildToExclusive(string? toDate)
    {
        var parsedDate = InstantHelper.ParseFromNullableString(toDate);
        return parsedDate?.Plus(Duration.FromDays(1));
    }
}
