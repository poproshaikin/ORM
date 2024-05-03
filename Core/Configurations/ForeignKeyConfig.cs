namespace ORM_0._3.Core.Configurations;

public class ForeignKeyConfig
{
    public string ReferencedTableName;
    public string ForeignKeyColName;
    public string ReferencedPrimaryKey;

    public string BuildQuery()
    {
        return $"FOREIGN KEY ({ForeignKeyColName}) REFERENCES {ReferencedTableName}({ReferencedPrimaryKey})";
    }
}