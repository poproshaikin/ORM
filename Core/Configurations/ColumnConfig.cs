using System.Data;
using System.Reflection;
using ORM.Core.Extensions;
using ORM.Core.Attributes;
using ORM.Core.Enums;
using DbType = ORM.Core.Enums.DbType;

namespace ORM.Core.Configurations;

internal class ColumnConfig
{
    public string Name;
    public DbDataType DataType;
    public DbConstraint[] Constraints;
    public ForeignKeyConfig? FkConfig;
    
    public static ColumnConfig CreateModel(PropertyInfo info, DbType dbType)
    {
        ArgumentNullException.ThrowIfNull(info);

        var model = new ColumnConfig()
        {
            Name = info.Name,
            DataType = EnumsConverter.GetDataType(info.PropertyType, dbType),
            Constraints = Array.Empty<DbConstraint>()
        };

        if (info.GetCustomAttribute(typeof(ConstraintAttribute)) is ConstraintAttribute cnsAtr)
        {
            if (info.IsPrimaryKey() && EnumsConverter.IsPrimaryKeyTypeValid(model.DataType, dbType))
                throw new InvalidCastException($"Primary key cannot be ov type {info.PropertyType}");
            
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