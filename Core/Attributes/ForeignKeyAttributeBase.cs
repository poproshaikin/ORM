namespace ORM.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class ForeignKeyAttributeBase : Attribute
{
    /// <summary>
    /// Name of related table // имя таблицы 
    /// </summary>
    public string ReferencedTableName     { get; private set; }
    public string LazyLoadingPropertyName { get; private set; }

    protected ForeignKeyAttributeBase(string rtn, string llpn)
    {
        ReferencedTableName = rtn;
        LazyLoadingPropertyName = llpn;
    }
}