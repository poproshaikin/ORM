﻿using System.Data.Common;
using ORM.Core.Enums;
using ORM.Sqlite.Db;

namespace ORM.Core.Db;

public abstract class DbConnector
{
    public    string  ConnectionString { get; private set; }
    public    DbType DbType           { get; protected set; }
    protected DbConnection Connection;
    
    protected DbConnector()
    {
    }

    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
    }

    public void OpenConnection(string connectionString)
    {
        this.ValidateConnectionString(connectionString);

        ConnectionString = connectionString;
        this.OpenConnection();
    }

    public void AssignConnectionToCmd(DbCommand command)
    {
        command.Connection = Connection;
    }
    
    protected    abstract    void       OpenConnection();
    protected    abstract    void       ValidateConnectionString(string connectionString);

    public       abstract    void       ExecuteTransaction(Action<DbConnection> trnsAct);
    public       abstract    object?    ExecuteScalar(string query);
    public       abstract    void       ExecuteNonQuery(DbCommand command);
    public       abstract    void       ExecuteNonQuery(string query);
    public       abstract    void       ExecuteNonQuery(string query, Action<DbCommand> cmdAct);
    public       abstract    void       ExecuteReader(string query, Action<DbDataReader> rdrAct);
    public       abstract    TParam     ExecuteReader<TParam>(string query, Func<DbDataReader, TParam> rdrFunc);
    
    public static DbConnector GetConnector(DbType type)
    {
        return type switch
        {
            DbType.Sqlite => new SqliteConnector(),
            
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}