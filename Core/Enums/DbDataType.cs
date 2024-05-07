namespace ORM.Core.Enums;

internal enum DbDataType
{
    NULL       = 1,
    
    INTEGER    = 2,
    BIGINT     = 3,
    SMALLINT   = 4,
    SERIAL     = 5,
    BIGSERIAL  = 6,
    NUMERIC    = 7,
    DECIMAL    = 8,
    REAL       = 9,
    CHAR       = 10,
    VARCHAR    = 11,
    TEXT       = 12,
    DATE       = 13,
    TIME       = 14,
    BOOLEAN    = 15,
    BYTEA      = 16,
    JSON       = 17,
    ARRAY      = 18,
    BLOB       = 19,
    
    // REAL       = 3,
    // TEXT       = 4,
    // BLOB       = 5,
    // NUMERIC    = 6
}