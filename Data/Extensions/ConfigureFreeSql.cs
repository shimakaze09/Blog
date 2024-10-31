using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Extensions;

public static class ConfigureFreeSql
{
    public static void AddFreeSql(this IServiceCollection services, IConfiguration configuration)
    {
        var freeSql = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, configuration.GetConnectionString("SQLite"))
            .UseAutoSyncStructure(true)
            .Build();

        services.AddSingleton(freeSql);

        // Warehousing
        services.AddFreeRepository();
    }
}