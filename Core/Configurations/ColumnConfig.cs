using System.Data;
using System.Reflection;
using ORM_0._3.Core.Attributes;
using ORM_0._3.Core.Enums;
using ORM_0._3.Core.Extensions;

namespace ORM_0._3.Core.Configurations;

public class ColumnConfig
{
    public string Name;
    public DbDataType DataType;
    public DbConstraint[] Constraints;
    public ForeignKeyConfig? FkConfig;
    
    public static ColumnConfig CreateModel(PropertyInfo info, ORM_0._3.Core.Enums.DbType dbType)
    {
        ArgumentNullException.ThrowIfNull(info);

        var model = new ColumnConfig()
        {
            Name = info.Name,
            DataType = EnumsConverter.GetDataType(info.PropertyType),
            Constraints = Array.Empty<DbConstraint>()
        };

        if (info.GetCustomAttribute(typeof(ConstraintAttribute)) is ConstraintAttribute cnsAtr)
        {
            model.Constraints = cnsAtr.Constraints.ToArray();
        }
        else if (info.GetCustomAttribute(typeof(ForeignKeyAttributeBase)) is ForeignKeyAttributeBase fkAtr)
        {
            model.FkConfig = new ForeignKeyConfig
            {
                ReferencedTableName = fkAtr.ReferencedTableName,
                ForeignKeyColName = model.Name,
                ReferencedPrimaryKey = fkAtr.GetType().GetGenericArguments()[0].GetPrimaryKeyName() ??
                                    throw new MissingPrimaryKeyException()
            };
        }

        return model;
    }
}