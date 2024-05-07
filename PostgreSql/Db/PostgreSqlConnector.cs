using System.Data.Common;
using Npgsql;
using ORM.Core.Db;
using ORM.Core.Enums;

namespace ORM.PostgreSql.Db;

internal class PostgreSqlConnector : DbConnector
{
    protected override void OpenConnection()
    {
        base.DbType = DbType.PostgreSql;
    }

    protected override void ValidateConnectionString(string connectionString)
    {
        base.Connection = new NpgsqlConnection(base.ConnectionString);
        base.Connection.Open();
    }

    public override void ExecuteTransaction(Action<DbConnection> trnsAct)
    {
        throw new NotImplementedException();
    }

    public override object? ExecuteScalar(string query)
    {
        throw new NotImplementedException();
    }

    public override void ExecuteNonQuery(DbCommand command)
    {
        throw new NotImplementedException();
    }

    public override void ExecuteNonQuery(string query)
    {
        throw new NotImplementedException();
    }

    public override void ExecuteNonQuery(string query, Action<DbCommand> cmdAct)
    {
        throw new NotImplementedException();
    }

    public override void ExecuteReader(string query, Action<DbDataReader> rdrAct)
    {
        throw new NotImplementedException();
    }

    public override TParam ExecuteReader<TParam>(string query, Func<DbDataReader, TParam> rdrFunc)
    {
        throw new NotImplementedException();
    }
}