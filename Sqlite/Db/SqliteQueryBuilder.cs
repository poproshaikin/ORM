﻿using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using ORM.Core.Extensions;
using ORM.Core.Attributes;
using ORM.Core.Configurations;
using ORM.Core.Db;
using ORM.Core.Enums;
using ORM.Core.Exceptions;

namespace ORM.Sqlite.Db;

internal class SqliteQueryBuilder : DbQueryBuilder
{
    public SqliteQueryBuilder(DbConnector connector) : base(connector)
    {
    }
    
    public override DbCommand CreateTableIfNotExists(TableConfig config)
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

        return new SQLiteCommand(query);
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
        var pkName = typeof(TEntity).GetPrimaryKeyName();
        
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

    public override DbCommand RemoveEntity(string tableName, int id)
    {
        var pkColName = this.GetPrimaryKeyColumnName(tableName);
        return new SQLiteCommand($"DELETE FROM {tableName} WHERE {pkColName} = {id}");
    }

    public override DbCommand AddEntity<TEntity>(string tableName, TEntity entity)
    {
        var properties = GetPropertiesWithNonDefaultPk(entity);
        
        var columns = properties.Select(p => p.Name).ToList();
        var parameterNames = columns.Select(col => $"@{col}").ToList();
        var values = properties.Select(p => p.GetValue(entity)).ToArray();

        var columnsJoin = string.Join(", ", columns);
        var paramsJoin = string.Join(", ", parameterNames);

        var command = new SQLiteCommand()
        {
            CommandText = $"INSERT INTO {tableName}({columnsJoin}) VALUES({paramsJoin})"
        };
        
        for (int i = 0; i < properties.Count(); i++)
        {
            command.Parameters.AddWithValue(parameterNames[i], values[i]);
        }

        return command;
    }

    public override IEnumerable<DbCommand> UpdateEntity<TEntity>(string tableName, TEntity oldEntity, TEntity newEntity) 
    {
        ArgumentNullException.ThrowIfNull(oldEntity);
        ArgumentNullException.ThrowIfNull(newEntity);
        
        if (oldEntity.GetPrimaryKeyValue() != newEntity.GetPrimaryKeyValue())
            throw new InvalidDataException();

        var oldProps = oldEntity.GetType().GetDbProperties().ToArray();
        var newProps = oldEntity.GetType().GetDbProperties().ToArray();

        if (oldProps.Length != newProps.Length) throw new InvalidCastException();

        for (int i = 0; i < oldProps.Length; i++)
        {
            var oldValue = oldProps[i].GetValue(oldEntity);
            var newValue = newProps[i].GetValue(newEntity);

            if (oldValue == newValue) continue;

            var columnName = oldProps[i].Name;
            var pkColumnName = oldEntity.GetType().GetPrimaryKeyName();
            var pkValue = oldEntity.GetPrimaryKeyValue();

            var sql = $"UPDATE {tableName} SET {columnName} = @{columnName} WHERE {pkColumnName} = {pkValue}";

            var command = new SQLiteCommand(sql);
            command.Parameters.AddWithValue($"@{columnName}", newValue);

            yield return command;
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

    public override bool ExistsTable(string tableName)
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
    
    /// <summary>
    /// Returns all non virtual and primary key info if hasn't default value
    /// </summary>
    private static IEnumerable<PropertyInfo> GetPropertiesWithNonDefaultPk(object entity)
    {
        foreach (var prop in entity.GetType().GetProperties())
        {
            if (prop.IsPropertyVirtual()) continue;

            if (prop.IsPrimaryKey())
            {
                if ((int)prop.GetValue(entity) == 0) continue;
            }

            yield return prop;
        }
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