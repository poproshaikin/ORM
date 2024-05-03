using System.Reflection;
using ORM_0._3.Core.Attributes;
using ORM_0._3.Core.Enums;

namespace ORM_0._3.Core.Extensions;

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
                           .GetProperty(typeof(T).GetPrimaryKeyColumnName()!)
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

    public static string? GetPrimaryKeyColumnName(this Type type) => type.GetPrimaryKeyInfo()!.Name;
}