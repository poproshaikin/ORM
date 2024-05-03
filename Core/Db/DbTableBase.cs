using ORM_0._3.Core.Enums;
using ORM_0._3.Core.Extensions;
using ORM_0._3.Sqlite.Db;

namespace ORM_0._3.Core.Db;

public abstract class DbTableBase
{
    internal     string           TableName             { get; set; }
    internal     DbQueryBuilder   QueryBuilder          { get; private set; }
    protected    DbType           DbType                { get; private set; }
    public DbTransactionContainer TransactionContainer { get; private set; }

    protected DbTableBase()
    {
    }

    internal abstract void InitChangesTracker();
    
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

        TransactionContainer = container;
    }
}