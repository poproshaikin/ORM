using System.Data.Common;
using System.Data.SQLite;
using ORM.Core.Enums;

namespace ORM.Core.Db;

public class DbTransactionContainer
{
    protected DbConnector Connector;

    private List<DbCommand> _commands;
    public IEnumerable<DbCommand> Commands => _commands;

    public DbTransactionContainer(DbConnector connector)
    {
        ArgumentNullException.ThrowIfNull(connector);
        
        Connector = connector;
        _commands = new List<DbCommand>();
    }

    public void AddCommand(DbCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Connector.AssignConnectionToCmd(command);
        
        _commands.Add(command);
    }

    public void Commit()
    {
        Connector.ExecuteTransaction(connection =>
        {
            foreach (var cmd in _commands)
            {
                Connector.ExecuteNonQuery(cmd);
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