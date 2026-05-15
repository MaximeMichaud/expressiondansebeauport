using Application.Interfaces.Services;
using Domain.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.SiteSettings.Reviews;

public class ReviewRequestBase
{
    public string Title { get; set; } = null!;
    public string Comment { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int Rating { get; set; }
}

public class ReviewRequestBaseValidator<T> : Validator<T> where T : ReviewRequestBase
{
    public ReviewRequestBaseValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(600);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
    }
}

public class CreateReviewRequest : ReviewRequestBase
{
}

public class CreateReviewValidator : ReviewRequestBaseValidator<CreateReviewRequest>
{
}

public class CreateReviewEndpoint : Endpoint<CreateReviewRequest, ReviewDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public CreateReviewEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService, IMapper mapper)
    {
        _context = context;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/site-settings/reviews");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateReviewRequest req, CancellationToken ct)
    {
        var settings = await _context.SiteSettings.FirstAsync(ct);
        var maxOrder = await _context.Reviews
            .Where(r => r.SiteSettingsId == settings.Id)
            .MaxAsync(r => (int?)r.SortOrder, ct) ?? -1;

        var review = new Review(settings.Id, req.Title, req.Comment, req.Author, req.Rating, maxOrder + 1);
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(ct);

        await _auditLogService.LogAsync("update", "site-settings", settings.Id, $"Review '{review.Title}' added (item: {review.Id}).");
        await Send.OkAsync(_mapper.Map<ReviewDto>(review), cancellation: ct);
    }
}

public class UpdateReviewRequest : ReviewRequestBase
{
    public Guid Id { get; set; }
}

public class UpdateReviewValidator : ReviewRequestBaseValidator<UpdateReviewRequest>
{
}

public class UpdateReviewEndpoint : Endpoint<UpdateReviewRequest, ReviewDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public UpdateReviewEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService, IMapper mapper)
    {
        _context = context;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Patch("admin/site-settings/reviews/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateReviewRequest req, CancellationToken ct)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == req.Id, ct);
        if (review is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        review.SetTitle(req.Title);
        review.SetComment(req.Comment);
        review.SetAuthor(req.Author);
        review.SetRating(req.Rating);
        await _context.SaveChangesAsync(ct);

        await _auditLogService.LogAsync("update", "site-settings", review.SiteSettingsId, $"Review '{review.Title}' updated (item: {review.Id}).");
        await Send.OkAsync(_mapper.Map<ReviewDto>(review), cancellation: ct);
    }
}

public class DeleteReviewRequest
{
    public Guid Id { get; set; }
}

public class DeleteReviewEndpoint : Endpoint<DeleteReviewRequest, EmptyResponse>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public DeleteReviewEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/site-settings/reviews/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteReviewRequest req, CancellationToken ct)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == req.Id, ct);
        if (review is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var details = $"Review '{review.Title}' deleted.";
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(ct);
        await _auditLogService.LogAsync("update", "site-settings", review.SiteSettingsId, $"{details} (item: {review.Id}).");
        await Send.NoContentAsync(ct);
    }
}
