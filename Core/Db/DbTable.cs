using System.Collections;
using ORM.Core.Enums;
using ORM.Core.Extensions;

namespace ORM.Core.Db;

public class DbTable<TEntity> : DbTableBase, IEnumerable<TEntity>, IList<TEntity> where TEntity : class
{
    private ChangesTracker<TEntity> _tracker;
    private List<TEntity?> _collection;

    public DbTable()
    {
    }

    internal override void InitChangesTracker()
    {
        if (base.QueryBuilder == null) throw new NullReferenceException();

        _tracker = new ChangesTracker<TEntity>(this);
    }
    internal override void InitCollection()
    {
        var entities = base.QueryBuilder.GetEntities<TEntity>(base.TableName).ToList();

        _collection = entities.ToList();
    } 
    public override void SaveChanges()
    {
        _tracker.SaveChanges();
        this.InitCollection();
    }

    public TEntity? At(int id)
    {
        var entity = _collection[id];
        
        _tracker.AddObject(entity, ContextModelStatus.Got);

        return entity;
    }

    public TEntity? AtPrimaryKey(int id)
    {
        var entity = _collection.FirstOrDefault(e =>
            e.GetPrimaryKeyValue() == base.QueryBuilder.GetEntity<TEntity>(base.TableName, id).GetPrimaryKeyValue());
        
        _tracker.AddObject(entity, ContextModelStatus.Got);

        return entity;
    }
    
    #region IList<TEntity> implementation

    public TEntity this[int index]
    {
        get => this.At(index) ?? throw new ArgumentOutOfRangeException(nameof(index));
        set => throw new NotImplementedException();
    }
    
    public int IndexOf(TEntity item)
    {
        for (int i = 0; i < _collection.Count; i++)
        {
            if (_collection[i] == item)
                return i;
        }

        return -1;
    }

    public void Insert(int index, TEntity item)
    {
        item.SetPrimaryKeyValue(index);
        
        _tracker.AddObject(item, ContextModelStatus.Added);
        _collection.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _tracker.AddObject(_collection[index], ContextModelStatus.Removed);
        _collection.RemoveAt(index);
    }

    public void Add(TEntity item)
    {
        _tracker.AddObject(item, ContextModelStatus.Added);
        _collection.Add(item);
    }

    public void Clear()
    {
        foreach (var entity in _collection)
        {
            _tracker.AddObject(entity, ContextModelStatus.Removed);
        }

        _collection.Clear();
    }

    public bool Contains(TEntity item)
    {
        return _collection.Contains(item);
    }

    public void CopyTo(TEntity[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(TEntity item)
    {
        var entity = _collection.FirstOrDefault(i => i.GetPrimaryKeyValue() == item.GetPrimaryKeyValue());

        if (entity == null) return false;
        
        _tracker.AddObject(entity, ContextModelStatus.Removed);
        _collection.Remove(item);

        return true;
    }

    public int Count => _collection.Count;
    public bool IsReadOnly { get; }
    
    #endregion

    #region IEnumerable implementation
    
    public IEnumerator<TEntity> GetEnumerator()
    {
        return new DbTableEnumerator<TEntity>(_collection);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    #endregion
}

file class DbTableEnumerator<TEntity> : IEnumerator<TEntity>
{
    private TEntity[] _collection;
    private int _currentIndex = -1;
    
    public DbTableEnumerator(IEnumerable<TEntity> collection)
    {
        _collection = collection.ToArray();
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public bool MoveNext()
    {
        _currentIndex++;
        return _currentIndex < _collection.Length;
    }

    public void Reset()
    {
        _currentIndex = -1;
    }

    public TEntity Current => _collection[_currentIndex];

    object IEnumerator.Current => Current;
}