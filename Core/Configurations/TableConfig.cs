using System.Data;
using ORM.Core.Extensions;
using DbType = ORM.Core.Enums.DbType;

namespace ORM.Core.Configurations;

internal class TableConfig
{
    public string Name;
    public List<ColumnConfig> Columns;

    public static TableConfig Build(string tableName, Type entityType, DbType dbType)
    {
        if (!entityType.HasPrimaryKey()) throw new MissingPrimaryKeyException();
        
        var props = entityType.GetProperties();

        if (props.Length == 0) throw new ArgumentException();

        var table = new TableConfig
        {
            Name = tableName,
            Columns = new List<ColumnConfig>()
        };
        
        foreach (var prop in props)
        {
            if (prop.IsPropertyVirtual()) continue;
            
            table.Columns.Add(ColumnConfig.CreateModel(prop, dbType));
        }

        return table;
    }
}