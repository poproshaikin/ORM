using System.Data.Common;
using System.Data.SQLite;
using ORM_0._3.Core.Enums;
using ORM_0._3.Sqlite.Db;

namespace ORM_0._3.Core.Db;

public class DbTransactionContainer
{
    protected DbConnector Connector;

    private List<string> _queries;
    public IEnumerable<string> Queries => _queries;

    public DbTransactionContainer(DbConnector connector)
    {
        ArgumentNullException.ThrowIfNull(connector);
        
        Connector = connector;
        _queries = new List<string>();
    }

    public void AddQuery(string query)
    {
        ArgumentNullException.ThrowIfNull(query);
        
        _queries.Add(query);
    }

    public void Commit()
    {
        Connector.ExecuteTransaction(connection =>
        {
            foreach (var query in _queries)
            {
                var trans = connection.BeginTransaction();
                
                using var cmd = connection.CreateCommand();
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
        });
    }

    private static DbCommand GetCorrespongingCommand(DbType type)
    {
        return type switch
        {
            DbType.Sqlite => new SQLiteCommand(),

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}