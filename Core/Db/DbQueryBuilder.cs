using ORM_0._3.Core.Configurations;
using ORM_0._3.Sqlite.Db;

namespace ORM_0._3.Core.Db;

public abstract class DbQueryBuilder
{
    protected DbConnector Connector { get; private set; }

    protected DbQueryBuilder(DbConnector connector)
    {
        Connector = connector;
    }

    public    DbConnector GetConnector() => Connector;
    
    public    abstract    string   CreateTableIfNotExists(TableConfig config);
    
    public    abstract    TEntity? GetEntity<TEntity>(string tableName, int id) where TEntity : class;
    public    abstract    IEnumerable<TEntity?> GetEntities<TEntity>(string tableName) where TEntity : class;
    
    public    abstract    string   RemoveEntity(string tableName, int id);
    public    abstract    string   AddEntity<TEntity>(string tableName, TEntity entity);
    public    abstract    IEnumerable<string> UpdateEntity<TEntity>(string tableName, TEntity oldEntity, TEntity newEntity);

    public    abstract    string   GetPrimaryKeyColumnName(string tableName);
    protected abstract    string   GetObjectJson(string tableName, int id);
    protected abstract    bool     ExistsTable(string tableName);
    // protected abstract    int      GetPkLastValue(string tableName);
    // protected abstract    IEnumerable<string> GetColumnNames(string tableName);

    public static DbQueryBuilder GetBuilder(DbConnector connector)
    {
        return connector switch
        {
            SqliteConnector => new SqliteQueryBuilder(connector),
            
            _ => throw new ArgumentOutOfRangeException(nameof(connector), connector, null)
        };
    }
    
    
}