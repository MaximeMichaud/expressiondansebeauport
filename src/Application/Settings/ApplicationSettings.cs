namespace Application.Settings;

public class ApplicationSettings
{
    public string BaseUrl { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    public string ErrorNotificationDestination { get; set; } = null!;
    public int TwoFactorAuthenticationDayDelay { get; set; }
}