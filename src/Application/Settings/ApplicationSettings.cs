namespace Application.Settings;

public class ApplicationSettings
{
    public string BaseUrl { get; set; } = default!;
    public string RedirectUrl { get; set; } = default!;
    public string ErrorNotificationDestination { get; set; } = default!;
    public int TwoFactorAuthenticationDayDelay { get; set; }
}