namespace ORM.Core.Enums;

public static class EnumsConverter
{
    public static DbDataType GetDataType(Type type)
    {
        return type switch
        {
            not null when type == typeof(string) => DbDataType.TEXT,
            not null when type == typeof(int)    => DbDataType.INTEGER,
            not null when type == typeof(double) => DbDataType.REAL,
            not null when type == typeof(float)  => DbDataType.REAL,
            not null when type == typeof(byte[]) => DbDataType.BLOB,

            _ => throw new ArgumentOutOfRangeException()
        };
    }
    public static string GetConstraintString(DbConstraint cns)
    {
        return cns switch
        {
            DbConstraint.PrimaryKey => "PRIMARY KEY",
            DbConstraint.Unique     => "UNIQUE",
            DbConstraint.NotNull    => "NOT NULL",
            
            _ => throw new ArgumentOutOfRangeException(nameof(cns))
        };
    }
    public static string GetDataTypeString(DbDataType type)
    {
        return type switch
        {
            DbDataType.NULL    => "NULL",
            
            DbDataType.INTEGER => "INTEGER",
            DbDataType.REAL    => "REAL",
            DbDataType.TEXT    => "TEXT",
            DbDataType.BLOB    => "BLOB",
            DbDataType.NUMERIC => "NUMERIC",
            
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}