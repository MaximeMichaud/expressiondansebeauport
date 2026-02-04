namespace Infrastructure.Mailing;

public class SendGridError
{
    public string Message { get; set; } = default!;
    public string Field { get; set; } = default!;
    public string Help { get; set; } = default!;
}