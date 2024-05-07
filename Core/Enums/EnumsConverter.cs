namespace ORM.Core.Enums;

internal static class EnumsConverter
{
    public static DbDataType GetDataType(Type type, DbType dbType)
    {
        return dbType switch
        {
            DbType.Sqlite => type switch
            {
                not null when type == typeof(string) => DbDataType.TEXT,
                not null when type == typeof(ushort) => DbDataType.INTEGER,
                not null when type == typeof(short) => DbDataType.INTEGER,
                not null when type == typeof(uint) => DbDataType.INTEGER,
                not null when type == typeof(int) => DbDataType.INTEGER,
                not null when type == typeof(long) => DbDataType.INTEGER,
                not null when type == typeof(ulong) => DbDataType.INTEGER,
                not null when type == typeof(double) => DbDataType.REAL,
                not null when type == typeof(float) => DbDataType.REAL,
                not null when type == typeof(byte[]) => DbDataType.BLOB,

                _ => throw new ArgumentOutOfRangeException(nameof(type),
                    message: "This data type isn't supported by ORM")
            },
            DbType.Postgres => type switch
            {
                not null when type == typeof(string) => DbDataType.TEXT,
                not null when type == typeof(short) => DbDataType.SMALLINT,
                not null when type == typeof(int) => DbDataType.INTEGER,
                not null when type == typeof(long) => DbDataType.BIGINT,
                not null when type == typeof(double) => DbDataType.REAL,
                not null when type == typeof(float) => DbDataType.REAL,
                not null when type == typeof(decimal) => DbDataType.DECIMAL,
                not null when type == typeof(char) => DbDataType.CHAR,
                not null when type == typeof(DateOnly) => DbDataType.DATE,
                not null when type == typeof(TimeOnly) => DbDataType.TIME,
                not null when type == typeof(bool) => DbDataType.BOOLEAN,
                not null when type == typeof(byte[]) => DbDataType.BYTEA,
                not null when type == typeof(Array) => DbDataType.ARRAY,

                // TODO not null when type == typeof(Json) => __,

                _ => throw new ArgumentOutOfRangeException(nameof(type),
                    message: "This data type isn't supported by ORM")
            },
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
        return Enum.GetName(typeof(DbDataType), type) ?? throw new ArgumentOutOfRangeException(nameof(type));
    }

    public static bool IsPrimaryKeyTypeValid(DbDataType pkType, DbType dbType)
    {
        return dbType switch
        {
            DbType.Sqlite => pkType switch
            {
                DbDataType.INTEGER => true,
                DbDataType.TEXT => true,
                DbDataType.BLOB => true,

                _ => false
            },
            DbType.Postgres => pkType switch
            {
                DbDataType.SMALLINT => true,
                DbDataType.INTEGER => true,
                DbDataType.BIGINT => true,
                DbDataType.TEXT => true,
                DbDataType.VARCHAR => true,
                DbDataType.SERIAL => true,
                DbDataType.BIGSERIAL => true,

                _ => false
            },

            _ => throw new ArgumentOutOfRangeException(nameof(dbType))
        };
    }
}