using System.Data.Common;
using ORM.Core.Configurations;
using ORM.Core.Db;

namespace ORM.PostgreSql.Db;

internal class PostgreSqlQueryBuilder : DbQueryBuilder
{
    public PostgreSqlQueryBuilder(DbConnector connector) : base(connector)
    {
    }

    public override DbCommand CreateTableIfNotExists(TableConfig config)
    {
        throw new NotImplementedException();
    }

    public override TEntity? GetEntity<TEntity>(string tableName, int id) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<TEntity?> GetEntities<TEntity>(string tableName) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public override DbCommand RemoveEntity(string tableName, int id)
    {
        throw new NotImplementedException();
    }

    public override DbCommand AddEntity<TEntity>(string tableName, TEntity entity)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<DbCommand> UpdateEntity<TEntity>(string tableName, TEntity oldEntity, TEntity newEntity)
    {
        throw new NotImplementedException();
    }

    public override string GetPrimaryKeyColumnName(string tableName)
    {
        throw new NotImplementedException();
    }

    protected override string GetObjectJson(string tableName, int id)
    {
        throw new NotImplementedException();
    }

    public override bool ExistsTable(string tableName)
    {
        throw new NotImplementedException();
    }
}