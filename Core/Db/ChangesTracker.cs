using ORM_0._3.Core.Enums;
using ORM_0._3.Core.Extensions;

namespace ORM_0._3.Core.Db;

public class ChangesTracker<TEntity> where TEntity : class
{
    private Dictionary<TEntity, ContextModelStatus> _changes;
    private DbTableBase _table;
    private DbQueryBuilder _builder;
    private DbTransactionContainer _container;

    public ChangesTracker(DbTableBase table)
    {
        _changes = new Dictionary<TEntity, ContextModelStatus>();
        _table = table;
        _builder = table.QueryBuilder;
        _container = table.TransactionContainer ?? new DbTransactionContainer(_builder.DbConnector);
    }

    public void AddObject(TEntity? obj, ContextModelStatus status)
    {
        if (obj == null) return;

        if (_changes.ContainsKey(obj)) return;
        
        _changes[obj] = status;
    }

    public void SaveChanges()
    {
        foreach (var kvp in _changes)
        {
            if (kvp.Value == ContextModelStatus.Added)
            {
                var command = _builder.AddEntity<TEntity>(_table.TableName, kvp.Key);
                
                _container.AddCommand(command);
            }
            else if (kvp.Value == ContextModelStatus.Removed)
            {
                var command = _builder.RemoveEntity(_table.TableName, 
                                                  (int)(kvp.Key.GetType().GetPrimaryKeyInfo().GetValue(kvp.Key)));
                _container.AddCommand(command);
            }
            else if (kvp.Value == ContextModelStatus.Got)
            {
                var oldEntity = _builder.GetEntity<TEntity>(_table.TableName,
                                                         kvp.Key.GetPrimaryKeyValue());
                var commands = _builder.UpdateEntity(_table.TableName, oldEntity, kvp.Key);
                foreach (var command in commands)
                {
                    _container.AddCommand(command);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            _changes.Remove(kvp.Key);
        }

        _container.Commit();
    }
}