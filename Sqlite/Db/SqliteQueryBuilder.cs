using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using ORM_0._3.Core.Attributes;
using ORM_0._3.Core.Configurations;
using ORM_0._3.Core.Db;
using ORM_0._3.Core.Enums;
using ORM_0._3.Core.Exceptions;
using ORM_0._3.Core.Extensions;

namespace ORM_0._3.Sqlite.Db;

public class SqliteQueryBuilder : DbQueryBuilder
{
    public SqliteQueryBuilder(DbConnector connector) : base(connector)
    {
    }
    
    public override string CreateTableIfNotExists(TableConfig config)
    {
        string query = $"CREATE TABLE IF NOT EXISTS {config.Name}(";

        List<ForeignKeyConfig> fkList = new List<ForeignKeyConfig>();
        
        for (int i = 0; i < config.Columns.Count; i++)
        {
            if (config.Columns[i].FkConfig is { } fkConfig)
            {
                fkList.Add(fkConfig);
            }
            
            query += $"{config.Columns[i].Name} " +
                     $"{EnumsConverter.GetDataTypeString(config.Columns[i].DataType)} ";
            
            if (config.Columns[i].Constraints.Length > 0)
            {
                foreach (var cns in config.Columns[i].Constraints)
                {
                    query += EnumsConverter.GetConstraintString(cns) + " ";
                }
            }

            if (i < config.Columns.Count - 1)
            {
                query += ", ";  
            }
        }

        if (fkList.Count > 0) query += ", ";
        
        for (int i = 0; i < fkList.Count; i++)
        {
            query += fkList[i].BuildQuery() + " ";

            if (i != fkList.Count - 1)
            {
                query += ", ";
            }
        }

        query += ")";

        return query;
    }

    public override TEntity? GetEntity<TEntity>(string tableName, int id) where TEntity : class
    {
        var entity = JsonConvert.DeserializeObject<TEntity>(this.GetObjectJson(tableName, id));

        if (entity == null) return null;

        var foreignKeys = entity.GetType().GetForeignKeys();
        
        foreach (var fk in foreignKeys)
        {
            var fkAtr = (fk.GetCustomAttribute(typeof(ForeignKeyAttributeBase)) as ForeignKeyAttributeBase)!;
            
            var foreignId = Convert.ToInt32(fk.GetValue(entity));
            var foreignTable = fkAtr.ReferencedTableName;
            var propertyToLazyLoad = entity.GetType().GetProperty(fkAtr.LazyLoadingPropertyName);

            if (propertyToLazyLoad == null)
                throw new PropertyNotFoundException(fkAtr.LazyLoadingPropertyName, typeof(TEntity));

            if (!propertyToLazyLoad.IsPropertyVirtual())
                throw new InvalidOperationException("Cannot lazy load related entity to non-virtual property");
            
            propertyToLazyLoad.SetValue(entity, 
                JsonConvert.DeserializeObject(this.GetObjectJson(foreignTable, foreignId),
                    propertyToLazyLoad.PropertyType));
        }

        return entity;
    }

    public override IEnumerable<TEntity?> GetEntities<TEntity>(string tableName) where TEntity : class
    {
        var pkName = typeof(TEntity).GetPrimaryKeyColumnName();
        
        string sql = $"SELECT {pkName} FROM {tableName}";

        List<int> ids = new List<int>();
        
        base.Connector.ExecuteReader(sql, reader =>
        {
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                ids.Add(id);
            }
        });
        
        foreach (var id in ids)
        {
            yield return JsonConvert.DeserializeObject<TEntity>(this.GetObjectJson(tableName, id));
        }
    }

    public override string RemoveEntity(string tableName, int id)
    {
        var pkColName = this.GetPrimaryKeyColumnName(tableName);
        return $"DELETE FROM {tableName} WHERE {pkColName} = {id}";
    }

    public override string AddEntity<TEntity>(string tableName, TEntity entity)
    {
        var properties = entity.GetType()
            .GetProperties()
            .Where(p =>
            {
                var attributes = p.GetCustomAttributes() as Attribute[] 
                                 ?? p.GetCustomAttributes()
                                     .ToArray();
                if (attributes.Length != 0)
                {
                    var cnsAtr = attributes.ToList()[0] as ConstraintAttribute;

                    return !cnsAtr.Constraints.Contains(DbConstraint.PrimaryKey);
                }

                return true;
            });
        
        string columns = string.Join(", ", properties.Select(p => p.Name));
        string values = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        string sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

        using var sqliteCmd = new SQLiteCommand(sql);

        foreach (var prop in properties)
        {
            sqliteCmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity));
        }

        Console.WriteLine("SqliteQueryBuilder method AddEntity()");
        Console.WriteLine($"Query: {sqliteCmd.CommandText}");
        
        return sqliteCmd.CommandText;
    }

    public override IEnumerable<string> UpdateEntity<TEntity>(string tableName, TEntity oldEntity, TEntity newEntity)
    {
        ArgumentNullException.ThrowIfNull(oldEntity);
        ArgumentNullException.ThrowIfNull(newEntity);
        
        if ((int)oldEntity.GetType()
                .GetPrimaryKeyInfo()!
                .GetValue(oldEntity)!
            !=
            (int)newEntity.GetType()
                .GetPrimaryKeyInfo()!
                .GetValue(newEntity)!)
            throw new InvalidDataException();

        var oldProps = oldEntity.GetType().GetProperties();
        var newProps = newEntity.GetType().GetProperties();

        for (int i = 0; i < oldProps.Length; i++)
        {
            var oldValue = oldProps[i].GetValue(oldEntity);
            var newValue = newProps[i].GetValue(newEntity);

            if (oldValue == newValue) continue;
            
            var columnName = oldProps[i].Name;
            var pkColumnName = this.GetPrimaryKeyColumnName(tableName);
            
            var pkValue = oldEntity.GetType()
                                         .GetPrimaryKeyInfo()!
                                         .GetValue(oldEntity);
            var sql =
                $"UPDATE {tableName} SET {columnName} = @{columnName} WHERE {pkColumnName} = {pkValue}";

            using var sqliteCmd = new SQLiteCommand();
            sqliteCmd.Parameters.AddWithValue(columnName, newValue);

            yield return sqliteCmd.CommandText;
        }
    }

    protected override string GetObjectJson(string tableName, int id)
    {
        string pkColumnName = this.GetPrimaryKeyColumnName(tableName);
        string sql = $"SELECT * FROM {tableName} WHERE {pkColumnName} = {id}";
        
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);

        using (var writer = new JsonTextWriter(sw))
        {
            writer.WriteStartObject();
            
            Connector.ExecuteReader(sql, reader =>
            {
                if (!reader.Read()) return;
                
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    writer.WritePropertyName(reader.GetName(i));   
                    writer.WriteValue(reader[i]);   
                }
            });

            writer.WriteEndObject();
        }
        
        return sw.ToString();
    }

    protected override bool ExistsTable(string tableName)
    {
        string sql = $"SELECT name FROM sqlite_master WHERE type = 'table'";

        bool exists = false;

        base.Connector.ExecuteReader(sql, reader =>
        {
            while (reader.Read())
            {
                if (reader["name"].ToString() == tableName)
                {
                    exists = true;
                }
            }
        });

        return exists;
    }

    public override string GetPrimaryKeyColumnName(string tableName)
    {
        var sql = $"PRAGMA table_info({tableName})";

        return base.Connector.ExecuteReader<string>(sql, reader =>
        {
            while (reader.Read())
            {
                if ((long)reader["pk"] == 1)
                {
                    return (string)reader["name"];
                }
            }

            throw new MissingPrimaryKeyException();
        });
    }

    // protected override int GetPkLastValue(string tableName)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // protected override IEnumerable<string> GetColumnNames(string tableName)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // protected override bool ExistsTable(string tableName)
    // {
    //     throw new NotImplementedException();
    // }
}