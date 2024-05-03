using ORM_0._3.Core.Db;
using ORM_0._3.Core.Enums;
using System.Reflection;
using ORM_0._3.Core.Configurations;
using ORM_0._3.Core.Db;

namespace ORM_0._3.Core;

public abstract class OrmContext
{
    protected abstract DbType DbType { get; set; }
    protected abstract string ConnectionString { get; set; }

    private List<DbTableBase> _tables; 
    private DbConnector Connector;
    private DbTransactionContainer _container;

    protected OrmContext()
    {
        if (ConnectionString == default ||
            DbType == default)
            throw new NullReferenceException("ConnectionString or DbType properties in derived class cannot be uninitialized before using");

        Connector = DbConnector.GetConnector(DbType);
        Connector.OpenConnection(ConnectionString);
        
        _tables = new List<DbTableBase>();
        _container = new DbTransactionContainer(Connector);

        this.InitTables();
    }

    private void InitTables()
    { 
        foreach (var prop in this.GetType().GetProperties())
        {
            if (!typeof(DbTableBase).IsAssignableFrom(prop.PropertyType))
                continue;

            var tableBase = (DbTableBase)Activator.CreateInstance(prop.PropertyType)!;
            
            tableBase.TableName = prop.Name;

            var config = TableConfig.Build(tableBase.TableName, prop.PropertyType.GetGenericArguments()[0], DbType);
            
            tableBase.SetQueryBuilder(DbQueryBuilder.GetBuilder(Connector));
            tableBase.InitChangesTracker();
            _container.AddQuery(
                tableBase
                    .QueryBuilder
                    .CreateTableIfNotExists(config));
            
            _tables.Add(tableBase);
            
            prop.SetValue(this, tableBase);
        }
        
        _container.Commit();
    }
}