namespace ORM.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute<TReferenced> : ForeignKeyAttributeBase
{
    public ForeignKeyAttribute(string referencedTableName,
        string lazyLoadingPropertyName)
        : base(referencedTableName, 
            lazyLoadingPropertyName)
    {
    }
}