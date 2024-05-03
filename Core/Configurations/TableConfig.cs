using System.Data;
using ORM_0._3.Core.Extensions;

namespace ORM_0._3.Core.Configurations;

public class TableConfig
{
    public string Name;
    public List<ColumnConfig> Columns;

    public static TableConfig Build(string tableName, Type entityType, ORM_0._3.Core.Enums.DbType dbType)
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