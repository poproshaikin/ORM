using ORM.Core.Configurations;
using ORM.Core.Db;
using ORM.Core.Enums;
using ORM.Core.Extensions;

namespace ORM.Core;

public abstract class OrmContext
{
    protected abstract DbType DbType { get; set; }
    protected abstract string ConnectionString { get; set; }

    private List<DbTableBase> _tables; 
    private DbConnector _connector;
    private DbTransactionContainer _container;

    protected OrmContext()
    {
        if (ConnectionString == default ||
            DbType == default)
            throw new NullReferenceException("ConnectionString or DbType properties in derived class cannot be uninitialized before using");

        _connector = DbConnector.GetConnector(DbType);
        _connector.OpenConnection(ConnectionString);
        
        _tables = new List<DbTableBase>();
        _container = new DbTransactionContainer(_connector);

        this.InitTables();
    }

    /// <summary>
    /// Initializes all properties of DbTable in a class inherited from this OrmContext
    /// </summary>
    private void InitTables()
    {
        foreach (var tableBase in this.GetAllTables())
        {
            tableBase.SetQueryBuilder(DbQueryBuilder.GetBuilder(_connector));
            tableBase.InitChangesTracker();
            
            if (!tableBase.Exists())
            {
                var config = TableConfig.Build(tableBase.TableName, tableBase.GetType().GetGenericArguments()[0], DbType);
                _container.AddCommand(tableBase
                                      .QueryBuilder
                                      .CreateTableIfNotExists(config));
            }
            else
            {
                tableBase.InitCollection();
            }
            
            _tables.Add(tableBase);
        }
        
        _container.Commit();
    }

    /// <summary>
    /// Saves changes made to entities in the context during the session
    /// </summary>
    public void SaveChanges()
    {
        foreach (var table in _tables)
        {
            table.SaveChanges();
        }
    }

    private IEnumerable<DbTableBase> GetAllTables()
    {
        foreach (var prop in this.GetType().GetProperties())
        {
            if (!typeof(DbTableBase).IsAssignableFrom(prop.PropertyType))
                continue;

            var table = (DbTableBase)Activator.CreateInstance(prop.PropertyType)!;
            table.TableName = prop.Name;

            prop.SetValue(this, table);
            
            yield return table;
        }
    }

    // private void GetDbColumns()
    // {
    //     var columnInfo = new DbColumnInfo()
    //     {
    //         Cid = _connector.
    //     }
    // }

    // protected void TryUpdateTables()
    // {
    //     foreach (var tableBase in _tables)
    //     {
    //         var type = tableBase.GetType().GetGenericArguments()[0];
    //
    //         var props = type.GetDbProperties();
    //         var cols = 
    //     }
    // }
}