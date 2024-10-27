namespace Data;

public class FreeSqlFactory
{
    public static IFreeSql Create(string connectionString)
    {
        return new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Sqlite, connectionString)
            .UseAutoSyncStructure(true)
            .Build();
    }
}