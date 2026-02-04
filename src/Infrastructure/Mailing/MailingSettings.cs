namespace Infrastructure.Mailing;

public class MailingSettings
{
    public string FromName { get; set; } = default!;
    public string FromAddress { get; set; } = default!;
    public string ToAddressForDevelopment { get; set; } = default!;
}