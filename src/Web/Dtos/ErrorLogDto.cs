namespace Web.Dtos;

public class ErrorLogDto
{
    public string Timestamp { get; set; } = null!;
    public string Level { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Exception { get; set; }
    public string? RequestId { get; set; }
    public string? SourceContext { get; set; }
}
