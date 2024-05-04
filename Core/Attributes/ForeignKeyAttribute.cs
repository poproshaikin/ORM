namespace ORM.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute<TRelated> : ForeignKeyAttributeBase
{
    public ForeignKeyAttribute(string relatedTableName,
        string lazyLoadingPropertyName)
        : base(relatedTableName, 
            lazyLoadingPropertyName)
    {
    }
}