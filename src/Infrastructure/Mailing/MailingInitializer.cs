using Application.Interfaces.Mailing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;

namespace Infrastructure.Mailing;

public static class MailingInitializer
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSendGrid(options => options.ApiKey = configuration["SendGrid:ApiKey"]);
        
        services.Configure<MailingSettings>(configuration.GetSection("Mailing"));
        services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));

        services.AddScoped<ISendGridMessageFactory, SendGridMessageFactory>();
        services.AddTransient<IEmailSender, SendGridSender>();
    }
}