namespace Infrastructure.Mailing;

public class MailingSettings
{
    public string FromName { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string ToAddressForDevelopment { get; set; } = null!;
}