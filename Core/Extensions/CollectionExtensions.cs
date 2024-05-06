using System.Reflection;

namespace ORM.Core.Extensions;

internal static class CollectionExtensions
{
    /// <summary>
    /// Returns first PropertyInfo that has PrimaryKey constraint attribute or null if not found
    /// </summary>
    /// <param name="props">Collection of PropertyInfos</param>
    public static PropertyInfo? GetPrimaryKey(this IEnumerable<PropertyInfo> props)
    {
        return props.FirstOrDefault(p => p.IsPrimaryKey());
    }
}