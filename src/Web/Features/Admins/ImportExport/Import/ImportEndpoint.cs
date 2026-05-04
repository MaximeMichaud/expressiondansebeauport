using System.Text.Json;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;

namespace Web.Features.Admins.ImportExport.Import;

public class ImportRequest
{
    public IFormFile File { get; set; } = null!;
    public bool ImportPages { get; set; } = true;
    public bool ImportSettings { get; set; } = true;
}

public class ImportValidator : Validator<ImportRequest>
{
    private const long MaxImportFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json",
        "text/json"
    };

    public ImportValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithErrorCode("FileRequired")
            .WithMessage("A file is required.");

        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithErrorCode("FileEmpty")
                .WithMessage("File cannot be empty.")
                .LessThanOrEqualTo(MaxImportFileSize)
                .WithErrorCode("FileTooLarge")
                .WithMessage("Import file must be 10 MB or less.");

            RuleFor(x => x.File)
                .Must(HaveJsonTypeAndExtension)
                .WithErrorCode("InvalidFileType")
                .WithMessage("Import file must be a JSON file.");
        });
    }

    private static bool HaveJsonTypeAndExtension(IFormFile? file)
    {
        if (file is null) return false;

        return string.Equals(Path.GetExtension(file.FileName), ".json", StringComparison.OrdinalIgnoreCase)
            && AllowedContentTypes.Contains(file.ContentType);
    }
}

public class ImportResponse
{
    public int PagesImported { get; set; }
    public bool SettingsImported { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ImportEndpoint : Endpoint<ImportRequest, ImportResponse>
{
    private readonly IPageRepository _pageRepository;
    private readonly ISiteSettingsRepository _settingsRepository;

    public ImportEndpoint(IPageRepository pageRepository, ISiteSettingsRepository settingsRepository)
    {
        _pageRepository = pageRepository;
        _settingsRepository = settingsRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/import");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
    }

    public override async Task HandleAsync(ImportRequest req, CancellationToken ct)
    {
        var response = new ImportResponse();

        using var stream = req.File.OpenReadStream();
        using var doc = await ParseImportDocument(stream, response, ct);
        if (doc is null)
        {
            await Send.ResponseAsync(response, StatusCodes.Status400BadRequest, ct);
            return;
        }

        var root = doc.RootElement;

        if (req.ImportPages && root.TryGetProperty("Pages", out var pagesEl))
        {
            foreach (var pageEl in pagesEl.EnumerateArray())
            {
                try
                {
                    var title = pageEl.GetProperty("Title").GetString() ?? "Untitled";
                    var slug = pageEl.GetProperty("Slug").GetString() ?? Page.GenerateSlug(title);
                    if (_pageRepository.SlugExists(slug))
                        slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

                    var page = new Page(title, slug);
                    if (pageEl.TryGetProperty("Content", out var contentEl))
                        page.SetContent(contentEl.GetString());
                    if (pageEl.TryGetProperty("MetaDescription", out var metaEl))
                        page.SetMetaDescription(metaEl.GetString());
                    if (pageEl.TryGetProperty("ContentMode", out var modeEl))
                    {
                        var mode = modeEl.GetString();
                        if (mode is "html" or "blocks")
                            page.SetContentMode(mode);
                    }
                    if (pageEl.TryGetProperty("Blocks", out var blocksEl))
                        page.SetBlocks(blocksEl.GetString());
                    if (pageEl.TryGetProperty("Status", out var statusEl) &&
                        statusEl.GetString() == "Published")
                        page.Publish();

                    await _pageRepository.Create(page);
                    response.PagesImported++;
                }
                catch (Exception ex)
                {
                    response.Errors.Add($"Page import error: {ex.Message}");
                }
            }
        }

        if (req.ImportSettings && root.TryGetProperty("SiteSettings", out var settingsEl))
        {
            try
            {
                var settings = await _settingsRepository.Get();
                if (settingsEl.TryGetProperty("SiteTitle", out var titleEl))
                    settings.SetSiteTitle(titleEl.GetString()!);
                if (settingsEl.TryGetProperty("Tagline", out var taglineEl))
                    settings.SetTagline(taglineEl.GetString());
                if (settingsEl.TryGetProperty("PrimaryColor", out var colorEl))
                    settings.SetPrimaryColor(colorEl.GetString()!);
                if (settingsEl.TryGetProperty("HeadingFont", out var hfEl))
                    settings.SetHeadingFont(hfEl.GetString()!);
                if (settingsEl.TryGetProperty("BodyFont", out var bfEl))
                    settings.SetBodyFont(bfEl.GetString()!);

                await _settingsRepository.Update(settings);
                response.SettingsImported = true;
            }
            catch (Exception ex)
            {
                response.Errors.Add($"Settings import error: {ex.Message}");
            }
        }

        await Send.OkAsync(response, cancellation: ct);
    }

    private static async Task<JsonDocument?> ParseImportDocument(Stream stream, ImportResponse response, CancellationToken ct)
    {
        try
        {
            return await JsonDocument.ParseAsync(stream, new JsonDocumentOptions
            {
                MaxDepth = 64,
                AllowTrailingCommas = true
            }, ct);
        }
        catch (JsonException)
        {
            response.Errors.Add("Fichier JSON invalide.");
            return null;
        }
    }
}
