using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using LiteDB;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.DatabaseAgent
{
  public class LiteDbAgent : ILiteDbAgent, IDisposable
  {
    private readonly LiteDatabase _db;

    public LiteDbAgent(IConfigurationStore configurationStore, ILoggerFactory loggerFactory)
    {
      var log = loggerFactory.CreateLogger(GetType());

      var connectionString = configurationStore.GetValueString("LITEDB_CONNECTION_STRING");

      // Leverage the LiteDB ConnectionString type to do the parameter composition for us.
      var connectionStringObj = new ConnectionString(connectionString);

      var dbPath = Path.GetDirectoryName(connectionStringObj.Filename);
      if (!Directory.Exists(dbPath))
      {
        Directory.CreateDirectory(dbPath);
      }

      log.LogInformation($"Initializing LiteDb with connection string: {connectionString}");

      _db = new LiteDatabase(connectionString);
    }

    public void DropTables(string[] tableNames)
    {
      foreach (var tablename in tableNames)
      {
        _db.DropCollection(tablename);
      }
    }

    /// <summary>
    /// Returns all records for a given table.
    /// </summary>
    public IEnumerable<T> GetTable<T>(string tableName) where T : MigrationObj => _db.GetCollection<T>(tableName).FindAll();

    /// <summary>
    /// Returns table entry by id or the most recently added if no id is provided.
    /// </summary>
    public T Find<T>(string tableName, long id = -1) where T : MigrationObj
    {
      return id == -1
        ? _db.GetCollection<T>(tableName).FindOne(Query.All(Query.Descending)) // Retrieve last added object.
        : _db.GetCollection<T>(tableName).FindById(id);
    }

    /// <summary>
    /// Inserts a new object into it's associated table.
    /// </summary>
    public long Insert<T>(T obj, string Tablename = null) where T : MigrationObj => _db.GetCollection<T>(obj.TableName ?? Tablename).Insert(obj).AsInt64;

    /// <summary>
    /// Returns all table entries where the predicate evaluates to true.
    /// </summary>
    public IEnumerable<MigrationObj> Find<T>(string tableName, Expression<Func<T, bool>> predicate) where T : MigrationObj => _db.GetCollection<T>(tableName).Find(predicate);

    /// <summary>
    /// Updates an object using the supplied Action delegate.
    /// </summary>
    public void Update<T>(long id, Action<T> action, string tableName = null) where T : MigrationObj
    {
      var objs = _db.GetCollection<T>(tableName);
      var dbObj = objs.FindById(id);

      action(dbObj);
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      objs.Update(dbObj);
    }

    public long UpdateOrInsert<T>(T obj, long? id = null, string tableName = null) where T : MigrationObj
    {
      var objs = _db.GetCollection<T>(tableName);
      var dbObj = objs.FindById(id);

      if (dbObj == null)
      {
        return _db.GetCollection<T>(tableName).Insert(obj).AsInt64;
      }

      if (!id.HasValue)
      {
        throw new Exception("Cannot update database object without valid Id."); 
      }

      dbObj = obj;
      objs.Update(dbObj);

      return Find<T>(tableName, id.Value).Id;
    }

    public void SetMigrationState(MigrationJob job, MigrationState migrationState, string message)
    {
      var projects = _db.GetCollection<MigrationProject>(Table.Projects);
      var dbObj = projects.FindOne(x => x.ProjectUid == job.Project.ProjectUID);

      dbObj.MigrationState = migrationState;
      dbObj.MigrationStateMessage = message;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void IncrementProjectMigrationCounter(Project project, int count = 1)
    {
      var projects = _db.GetCollection<MigrationProject>(Table.Projects);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.MigrationAttempts += count;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void SetResolveCSIBMessage(string tableName, string key, string message)
    {
      var projects = _db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == key);

      UpdateProject(projects, dbObj, () => dbObj.ResolveCSIBMessage = message);
    }

    public void SetProjectCSIB(string tableName, string key, string csib)
    {
      var projects = _db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == key);

      UpdateProject(projects, dbObj, () => dbObj.CSIB = csib);
    }

    private static void UpdateProject(LiteCollection<MigrationProject> projects, MigrationProject dbObj, Action action)
    {
      action();

      dbObj.DateTimeUpdated = DateTime.UtcNow;
      projects.Update(dbObj);
    }

    public void Dispose()
    {
      _db?.Dispose();
    }
  }
}
