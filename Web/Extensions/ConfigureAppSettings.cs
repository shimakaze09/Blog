using Web.Models.Config;

namespace Web.Extensions;

public static class ConfigureAppSettings
{
    public static void AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        // SecuritySettings
        services.Configure<SecuritySettings>(configuration.GetSection(nameof(SecuritySettings)));
    }
}