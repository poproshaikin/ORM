using ORM.Core.Enums;
using ORM.Sqlite.Db;

namespace ORM.Core.Db;

public abstract class DbTableBase
{
    internal     string         TableName       { get; set; }
    internal     DbQueryBuilder QueryBuilder    { get; private set; }
    protected    DbType         DbType          { get; private set; }
    internal DbTransactionContainer Container   { get; private set; }

    protected DbTableBase()
    {
    }

    /// <summary>
    /// Инициализирует коллекцию из TEntity в унаследованных классах обьектами из базы данных
    /// </summary>
    internal abstract void InitCollection();
    internal abstract void InitChangesTracker();
    public   abstract void SaveChanges();
    
    internal void SetQueryBuilder(DbQueryBuilder ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        
        QueryBuilder = ex;

        DbType = ex switch
        {
            SqliteQueryBuilder => DbType.Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(ex), ex, null)
        };
    }

    internal void SetTransactionContainer(DbTransactionContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        Container = container;
    }

    internal bool Exists()
    {
        return QueryBuilder.ExistsTable(TableName);
    }
}