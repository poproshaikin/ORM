using System.Data.Common;
using System.Data.SQLite;
using ORM.Core.Db;
using DbType = ORM.Core.Enums.DbType;

namespace ORM.Sqlite.Db;

public class SqliteConnector : DbConnector
{
    public SqliteConnector() : base()
    {
        base.DbType = DbType.Sqlite;
    }

    protected override void OpenConnection()
    {
        base.Connection = new SQLiteConnection(base.ConnectionString);
        base.Connection.Open();
    }
    protected override void ValidateConnectionString(string connectionString)
    {
        try
        {
            var conn = new SQLiteConnection(connectionString);

            conn.Open();
            conn.Close();
        }
        catch (SQLiteException e)
        {
            Console.WriteLine();
            Console.WriteLine($"  Error occured during connection string validation: {e.Message}");
            Console.WriteLine();
            throw;
        }
    }

    public override void ExecuteTransaction(Action<DbConnection> trnsAct)
    {
        using var sqliteTrns = (base.Connection.BeginTransaction() as SQLiteTransaction)!;

        try
        {
            trnsAct.Invoke(base.Connection);
            sqliteTrns.Commit();
        }
        catch
        {
            sqliteTrns.Rollback();
        }
    }
    
    public override object? ExecuteScalar(string query)
    {
        using var cmd = new SQLiteCommand(query, base.Connection as SQLiteConnection);
        return cmd.ExecuteScalar();
    }
    
    public override void ExecuteNonQuery(string query)
    {
        using var cmd  = new SQLiteCommand(query, base.Connection as SQLiteConnection);
        cmd.ExecuteNonQuery();
    }

    public override void ExecuteNonQuery(string query, Action<DbCommand> cmdAct)
    {
        using var cmd = new SQLiteCommand(query, base.Connection as SQLiteConnection);
        cmdAct.Invoke(cmd);
        cmd.ExecuteNonQuery();
    }

    public override void ExecuteNonQuery(DbCommand command) => command.ExecuteNonQuery();

    public override void ExecuteReader(string query, Action<DbDataReader> rdrAct)
    {
        using var cmd  = new SQLiteCommand(query, base.Connection as SQLiteConnection);
        using var reader = cmd.ExecuteReader();

        rdrAct.Invoke(reader);
    }
    public override TParam  ExecuteReader<TParam>(string query, Func<DbDataReader, TParam> rdrFunc)
    {
        using var cmd  = new SQLiteCommand(query, base.Connection as SQLiteConnection);
        using var reader = cmd.ExecuteReader();

        return rdrFunc.Invoke(reader);
    }
}