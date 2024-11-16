using Share.Utils;
using Web.Models.Config;

namespace Web.Extensions;

public static class ConfigureAppSettings
{
    public static void AddSettings(this IServiceCollection services, ConfigurationManager configuration)
    {
        configuration.AddJsonFile("appsettings-email.json", optional: true, reloadOnChange: true);
        // SecuritySettings
        services.Configure<Auth>(configuration.GetSection(nameof(Auth)));
        // Email configuration
        services.Configure<EmailAccountConfig>(configuration.GetSection(nameof(EmailAccountConfig)));
    }
}