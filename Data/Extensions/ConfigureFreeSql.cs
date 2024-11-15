using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Extensions;

public static class ConfigureFreeSql
{
    public static void AddFreeSql(this IServiceCollection services, IConfiguration configuration)
    {
        // var freeSql = FreeSqlFactory.Create(configuration.GetConnectionString("SQLite"));
        var freeSql = FreeSqlFactory.CreateMySql(configuration.GetConnectionString("MySql"));

        services.AddSingleton(freeSql);

        // Warehousing
        services.AddFreeRepository();
    }
}