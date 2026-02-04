namespace Web.Features.Common;

public class FormFile
{
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string BodyStream { get; set; } = null!;
}