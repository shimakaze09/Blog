using System.Diagnostics;
using FreeSql;
using FreeSql.Internal;

namespace Data;

public class FreeSqlFactory
{
    public static IFreeSql Create(DataType dataType, string connectionString)
    {
        return new FreeSqlBuilder()
            .UseConnectionString(dataType, connectionString)
            .UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower)
            .UseAutoSyncStructure(true) // Automatically synchronize entity structure to the database
            .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
            .Build(); // Please be sure to define it as a Singleton
    }

    public static IFreeSql Create(string connectionString)
    {
        return Create(DataType.Sqlite, connectionString);
    }

    public static IFreeSql CreateMySql(string connectionString)
    {
        return Create(DataType.MySql, connectionString);
    }

    public static IFreeSql CreatePostgresSql(string connectionString)
    {
        return Create(DataType.PostgreSQL, connectionString);
    }
}