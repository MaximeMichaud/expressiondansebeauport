namespace Web.Dtos;

public class SiteHealthDto
{
    public string OverallStatus { get; set; } = null!;
    public List<HealthCheckDto> Checks { get; set; } = new();
}

public class HealthCheckDto
{
    public string Name { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Details { get; set; }
}
