namespace Infrastructure.Mailing;

public class SendGridError
{
    public string Message { get; set; } = null!;
    public string Field { get; set; } = null!;
    public string Help { get; set; } = null!;
}