using System.Reflection;
using ORM.Core.Attributes;
using ORM.Core.Enums;

namespace ORM.Core.Extensions;

internal static class TypeExtensions
{
    /// <summary>
    /// Gets PropertyInfo instance of property in entityType parameter that has PrimaryKey constraint attribute
    /// </summary>
    /// <returns>PropertyInfo instance of property in entityType parameter that has PrimaryKey constraint attribute
    /// or null in case that any property hasn't this attribute</returns>
    public static PropertyInfo? GetPrimaryKeyInfo(this Type entityType)
    {
        var info = entityType.GetProperties().FirstOrDefault(IsPrimaryKey);
        return info;
    }

    /// <summary>
    /// Returns the vallue of property that has PrimaryKey constraint attribute
    /// </summary>
    public static int GetPrimaryKeyValue<T>(this T obj) => 
        Convert.ToInt32(obj.GetType()
                           .GetProperty(typeof(T).GetPrimaryKeyName()!)
                           .GetValue(obj));
    
    /// <summary>
    /// Returns true if entityType has any property that has PrimaryKey constraint attribute, otherwise false
    /// </summary>
    public static bool HasPrimaryKey(this Type entityType)
    {
        return entityType.GetProperties().Any(
            p => 
            {
                var atr = p.GetCustomAttribute(typeof(ConstraintAttribute)) as ConstraintAttribute;

                return atr != null &&
                       atr.Constraints.Contains(DbConstraint.PrimaryKey);;
            });
    }

    /// <summary>
    /// Returns true if property has PrimaryKey constraint attribute, otherwise false
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsPrimaryKey(this PropertyInfo info)
    {
        var atr = info.GetCustomAttribute(typeof(ConstraintAttribute)) as ConstraintAttribute;
        
        return atr != null &&
               atr.Constraints.Contains(DbConstraint.PrimaryKey);
    }
    
    /// <summary>
    /// Returns true if property parameter is virtual, otherwise false
    /// </summary>
    public static bool IsPropertyVirtual(this PropertyInfo property)
    {
        var getMethod = property.GetGetMethod();
        var setMethod = property.GetSetMethod();

        return (getMethod != null && getMethod.IsVirtual) || (setMethod != null && setMethod.IsVirtual);
    }
    
    /// <summary>
    /// Gets all properties that have ForeignKey constraint attribute
    /// </summary>
    public static IEnumerable<PropertyInfo> GetForeignKeys(this Type type) =>
        type.GetProperties()
            .Where
            (prop => 
                prop.GetCustomAttribute(typeof(ForeignKeyAttributeBase)) is ForeignKeyAttributeBase);
    
    
    public static IEnumerable<PropertyInfo> GetLazyLoadingProperties(this Type type) => 
        type.GetProperties()
            .Where(IsPropertyVirtual);

    public static string? GetPrimaryKeyName(this Type type) => type.GetPrimaryKeyInfo()!.Name;

    /// <summary>
    /// Returns all PropertyInfo of the type that can be added to the database.
    /// </summary>
    /// <returns>A collection of PropertyInfo that are not virtual.</returns>
    public static IEnumerable<PropertyInfo> GetDbProperties(this Type type)
    {
        return type.GetProperties()
            .Where(p => !p.IsPropertyVirtual());
    }

    /// <summary>
    /// Sets value parameter to entity's property that has PrimaryKey constraint attribute
    /// </summary>
    /// <param name="entity">Entity what's primary key will be set</param>
    /// <param name="value">Value so be assigned to primary key</param>
    public static void SetPrimaryKeyValue<TEntity>(this TEntity entity, object value)
    {
        var pkInfo = typeof(TEntity).GetPrimaryKeyInfo();
            pkInfo.SetValue(entity, value);
    }
}