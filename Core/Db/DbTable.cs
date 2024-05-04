using ORM.Core.Enums;

namespace ORM.Core.Db;

public class DbTable<TEntity> : DbTableBase where TEntity : class
{
    private ChangesTracker<TEntity> _tracker;

    public DbTable()
    {
    }

    internal override void InitChangesTracker()
    {
        if (base.QueryBuilder == null) throw new NullReferenceException();

        _tracker = new ChangesTracker<TEntity>(this);
    }

    public void SaveChanges()
    {
        _tracker.SaveChanges();
    }

    public TEntity? this[int id] => this.At(id);
    
    public List<TEntity> ToList()
    {
        var entities = base.QueryBuilder.GetEntities<TEntity>(base.TableName).ToList();

        foreach (var e in entities)
        {
            _tracker.AddObject(e, ContextModelStatus.Got);
        }

        return entities!;
    } 
    
    public TEntity? At(int id)
    {
        var entity = base.QueryBuilder.GetEntity<TEntity>(base.TableName, id);
        
        _tracker.AddObject(entity, ContextModelStatus.Got);

        return entity;
    }

    public void Add(TEntity item)
    {
        _tracker.AddObject(item, ContextModelStatus.Added);
    }

    public void Remove(int id)
    {
        _tracker.AddObject(
            base.QueryBuilder.GetEntity<TEntity>(base.TableName, id),
            ContextModelStatus.Removed);
    }
}