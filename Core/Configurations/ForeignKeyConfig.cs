namespace ORM.Core.Configurations;

internal class ForeignKeyConfig
{
    public string ReferencedTableName;
    public string ForeignKeyColName;
    public string ReferencedPrimaryKey;

    public string BuildQuery()
    {
        return $"FOREIGN KEY ({ForeignKeyColName}) REFERENCES {ReferencedTableName}({ReferencedPrimaryKey})";
    }
}