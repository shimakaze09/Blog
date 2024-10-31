using FreeSql;

namespace Data;

public class FreeSqlFactory
{
    public static IFreeSql Create(string connectionString)
    {
        return new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, connectionString)
            .UseAutoSyncStructure(true)
            .Build();
    }
}