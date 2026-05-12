using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Web.Dtos;

namespace Web.Features.Admins.Version;

public class GetAdminVersionEndpoint : EndpointWithoutRequest<AppVersionDto>
{
    private const string CacheKey = "admin:version:github-releases";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GetAdminVersionEndpoint> _logger;

    public GetAdminVersionEndpoint(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<GetAdminVersionEndpoint> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("admin/version");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var owner = _configuration["GitHubRepository:Owner"] ?? "MaximeMichaud";
        var name = _configuration["GitHubRepository:Name"] ?? "expressiondansebeauport";
        var token = _configuration["GitHubRepository:Token"];

        var current = ResolveCurrentVersion();

        var repository = new AppVersionRepositoryDto
        {
            Owner = owner,
            Name = name,
            HtmlUrl = $"https://github.com/{owner}/{name}",
            ReleasesUrl = $"https://github.com/{owner}/{name}/releases"
        };

        var (releases, updateError) = await GetReleasesAsync(owner, name, token, ct);

        var latest = releases.FirstOrDefault(r => !r.IsDraft && !r.IsPrerelease) ?? releases.FirstOrDefault();
        var isUpToDate = latest is null
            || current.SemanticVersion is null
            || CompareSemanticVersions(current.SemanticVersion, NormalizeTag(latest.TagName)) >= 0;

        await Send.OkAsync(new AppVersionDto
        {
            Current = current,
            Repository = repository,
            LatestRelease = latest,
            Releases = releases,
            IsUpToDate = isUpToDate,
            UpdateError = updateError,
            FetchedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        }, cancellation: ct);
    }

    private async Task<(List<AppVersionReleaseDto> Releases, string? Error)> GetReleasesAsync(
        string owner, string name, string? token, CancellationToken ct)
    {
        if (_cache.TryGetValue(CacheKey, out List<AppVersionReleaseDto>? cached) && cached is not null)
        {
            return (cached, null);
        }

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("expressiondansebeauport-admin", "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"https://api.github.com/repos/{owner}/{name}/releases?per_page=20";
            using var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Échec de récupération des releases GitHub: {Status} {Body}", response.StatusCode, body);
                return (new List<AppVersionReleaseDto>(), $"GitHub a répondu {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var raw = await JsonSerializer.DeserializeAsync<List<GithubReleaseRaw>>(stream, JsonOptions, ct);
            var releases = (raw ?? new List<GithubReleaseRaw>())
                .Select(MapRelease)
                .OrderByDescending(r => r.PublishedAt)
                .ToList();

            _cache.Set(CacheKey, releases, CacheDuration);
            return (releases, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue lors de la récupération des releases GitHub.");
            return (new List<AppVersionReleaseDto>(), "Impossible de joindre l'API GitHub.");
        }
    }

    private static AppVersionCurrentDto ResolveCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "0.0.0";

        string? sha = null;
        string semantic = informational;
        var plusIndex = informational.IndexOf('+');
        if (plusIndex >= 0)
        {
            semantic = informational[..plusIndex];
            sha = informational[(plusIndex + 1)..];
        }

        long builtAt;
        try
        {
            var location = assembly.Location;
            builtAt = !string.IsNullOrEmpty(location) && File.Exists(location)
                ? new DateTimeOffset(File.GetLastWriteTimeUtc(location)).ToUnixTimeSeconds()
                : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        catch
        {
            builtAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        return new AppVersionCurrentDto
        {
            Version = informational,
            SemanticVersion = semantic,
            CommitSha = sha,
            BuiltAt = builtAt
        };
    }

    private static AppVersionReleaseDto MapRelease(GithubReleaseRaw raw) => new()
    {
        TagName = raw.TagName ?? string.Empty,
        Name = string.IsNullOrWhiteSpace(raw.Name) ? raw.TagName ?? string.Empty : raw.Name,
        Body = raw.Body,
        HtmlUrl = raw.HtmlUrl ?? string.Empty,
        PublishedAt = raw.PublishedAt?.ToUnixTimeSeconds() ?? raw.CreatedAt?.ToUnixTimeSeconds() ?? 0,
        IsPrerelease = raw.Prerelease,
        IsDraft = raw.Draft,
        AuthorLogin = raw.Author?.Login ?? string.Empty
    };

    private static readonly System.Text.RegularExpressions.Regex PostReleaseRegex =
        new(@"^\d+-g[0-9a-f]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static string NormalizeTag(string tag) => tag.TrimStart('v', 'V');

    private static int CompareSemanticVersions(string installed, string latest)
    {
        var baseCmp = CompareVersionBase(installed, latest);
        if (baseCmp != 0) return baseCmp;

        var installedSuffix = ExtractSuffix(installed);
        var latestSuffix = ExtractSuffix(latest);

        if (installedSuffix is null) return latestSuffix is null ? 0 : 1;
        if (IsPostReleaseSuffix(installedSuffix)) return latestSuffix is null ? 1 : 0;
        return latestSuffix is null ? -1 : 0;
    }

    private static int CompareVersionBase(string a, string b)
    {
        var pa = ParseBase(a);
        var pb = ParseBase(b);
        for (var i = 0; i < 3; i++)
        {
            var cmp = pa[i].CompareTo(pb[i]);
            if (cmp != 0) return cmp;
        }
        return 0;
    }

    private static int[] ParseBase(string version)
    {
        var trimmed = version.TrimStart('v', 'V');
        var sepIndex = trimmed.IndexOfAny(new[] { '-', '+' });
        if (sepIndex >= 0) trimmed = trimmed[..sepIndex];
        var parts = trimmed.Split('.');
        var result = new[] { 0, 0, 0 };
        for (var i = 0; i < Math.Min(3, parts.Length); i++)
        {
            int.TryParse(parts[i], out result[i]);
        }
        return result;
    }

    private static string? ExtractSuffix(string version)
    {
        var trimmed = version.TrimStart('v', 'V');
        var dashIndex = trimmed.IndexOf('-');
        if (dashIndex < 0) return null;
        var plusIndex = trimmed.IndexOf('+');
        var endIndex = plusIndex >= 0 && plusIndex > dashIndex ? plusIndex : trimmed.Length;
        return trimmed[(dashIndex + 1)..endIndex];
    }

    private static bool IsPostReleaseSuffix(string suffix) => PostReleaseRegex.IsMatch(suffix);

    private class GithubReleaseRaw
    {
        public string? TagName { get; set; }
        public string? Name { get; set; }
        public string? Body { get; set; }
        public string? HtmlUrl { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public bool Prerelease { get; set; }
        public bool Draft { get; set; }
        public GithubAuthorRaw? Author { get; set; }
    }

    private class GithubAuthorRaw
    {
        public string? Login { get; set; }
    }
}
