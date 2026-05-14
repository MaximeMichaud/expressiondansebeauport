namespace Web.Dtos;

public class AppVersionDto
{
    public AppVersionCurrentDto Current { get; set; } = null!;
    public AppVersionRepositoryDto Repository { get; set; } = null!;
    public AppVersionReleaseDto? LatestRelease { get; set; }
    public List<AppVersionReleaseDto> Releases { get; set; } = new();
    public bool IsUpToDate { get; set; }
    public string? UpdateError { get; set; }
    public long FetchedAt { get; set; }
}

public class AppVersionCurrentDto
{
    public string Version { get; set; } = null!;
    public string? SemanticVersion { get; set; }
    public string? CommitSha { get; set; }
    public long BuiltAt { get; set; }
}

public class AppVersionRepositoryDto
{
    public string Owner { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string HtmlUrl { get; set; } = null!;
    public string ReleasesUrl { get; set; } = null!;
}

public class AppVersionReleaseDto
{
    public string TagName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Body { get; set; }
    public string HtmlUrl { get; set; } = null!;
    public long PublishedAt { get; set; }
    public bool IsPrerelease { get; set; }
    public bool IsDraft { get; set; }
    public string AuthorLogin { get; set; } = null!;
}
