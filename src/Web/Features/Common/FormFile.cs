namespace Web.Features.Common;

public class FormFile
{
    public string ContentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string BodyStream { get; set; } = default!;
}