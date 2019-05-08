using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using TCCToDataOcean.Utils;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.DatabaseAgent
{
  public class LiteDbAgent : ILiteDbAgent
  {
    private readonly LiteDatabase db;

    public LiteDbAgent(IConfigurationStore configurationStore, IEnvironmentHelper environmentHelper)
    {
      var databasePath = Path.Combine(Directory.GetCurrentDirectory(), "database");
      var databaseSuffix = environmentHelper.GetVariable("MIGRATION_TARGET", 1);

      if (!Directory.Exists(databasePath)) Directory.CreateDirectory(databasePath);

      db = new LiteDatabase(Path.Combine(databasePath, configurationStore.GetValueString("LITEDB_MIGRATION_DATABASE") + "-" + databaseSuffix + ".db"));
    }

    public IEnumerable<T> GetTable<T>(string tableName) => db.GetCollection<T>(tableName).FindAll();

    public void DropTables(string[] tableNames)
    {
      foreach (var tablename in tableNames)
      {
        db.DropCollection(tablename);
      }
    }

    public void InitDatabase()
    {
      db.GetCollection<MigrationInfo>(Table.MigrationInfo).Insert(new MigrationInfo());
    }

    public void SetMigationInfo_EndTime()
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.Find(x => x.Id == 1).First();

      var endTimeUtc = DateTime.Now;
      dbObj.EndTime = endTimeUtc;
      dbObj.Duration = endTimeUtc.Subtract(dbObj.StartTime).ToString();
      objs.Update(dbObj);
    }

    public void SetMigationInfo_SetProjectCount(int projectCount)
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.Find(x => x.Id == 1).First();

      dbObj.ProjectsTotal = projectCount;
      objs.Update(dbObj);
    }

    public void SetMigationInfo_SetEligibleProjectCount(int projectCount)
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.Find(x => x.Id == 1).First();

      dbObj.EligibleProjects = projectCount;
      objs.Update(dbObj);
    }

    public void SetMigationInfo_IncrementProjectsProcessed()
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.Find(x => x.Id == 1).First();

      dbObj.ProjectsCompleted += 1;
      objs.Update(dbObj);
    }

    public void WriteRecord(string tableName, Project project)
    {
      var objs = db.GetCollection<MigrationProject>(tableName);
      var dbObj = objs.Find(x => x.Id == project.LegacyProjectID).FirstOrDefault();

      if (dbObj == null)
      {
        db.GetCollection<MigrationProject>(tableName).Insert(new MigrationProject(project));
      }
      else
      {
        dbObj = new MigrationProject(project);
        objs.Update(dbObj);
      }
    }

    public void WriteError(string projectUid, string errorMessage)
    {
      db.GetCollection<MigrationErrorMessage>(Table.Errors).Insert(new MigrationErrorMessage(projectUid, errorMessage));
    }

    public void SetMigrationState(string tableName, Project project, MigrationState migrationState)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.MigrationState = migrationState;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void WriteRecord(string tableName, FileData file)
    {
      var objs = db.GetCollection<MigrationFile>(tableName);
      var dbObj = objs.Find(x => x.Id == file.LegacyFileId).FirstOrDefault();

      if (dbObj == null)
      {
        db.GetCollection<MigrationFile>(tableName).Insert(new MigrationFile(file));
      }
      else
      {
        dbObj = new MigrationFile(file);
        objs.Update(dbObj);
      }
    }

    public void SetMigrationState(string tableName, FileData file, MigrationState migrationState)
    {
      var files = db.GetCollection<MigrationFile>(tableName);
      var dbObj = files.FindOne(x => x.Id == file.LegacyFileId);

      dbObj.MigrationState = migrationState;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      files.Update(dbObj);
    }

    public void SetFileSize(string tableName, FileData file, long length)
    {
      var files = db.GetCollection<MigrationFile>(tableName);
      var dbObj = files.FindOne(x => x.Id == file.LegacyFileId);

      dbObj.Length = length;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      files.Update(dbObj);
    }

    public void SetProjectCoordinateSystemDetails(string tableName, Project project, bool isValid)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.DcFilename = project.CoordinateSystemFileName;
      dbObj.HasValidDcFile = isValid;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    private LiteCollection<T> GetCollection<T>(string tableName) => db.GetCollection<T>(tableName);

    public void SetProjectFilesDetails(string tableName, Project project, int totalFileCount, int eligibleFileCount)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.TotalFileCount = totalFileCount;
      dbObj.EligibleFileCount = eligibleFileCount;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void SetCanResolveCSIB(string tableName, string key, bool canResolveCsib)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == key);

      UpdateProject(projects, dbObj, () => dbObj.CanResolveCSIB = canResolveCsib);
    }

    public void SetResolveCSIBMessage(string tableName, string key, string message)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == key);

      UpdateProject(projects, dbObj, () => dbObj.ResolveCSIBMessage = message);
    }

    public void SetProjectCSIB(string tableName, string key, string csib)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == key);

      UpdateProject(projects, dbObj, () => dbObj.CSIB = csib);
    }

    private static void UpdateProject(LiteCollection<MigrationProject> projects, MigrationProject dbObj, Action action)
    {
      action();

      dbObj.DateTimeUpdated = DateTime.UtcNow;
      projects.Update(dbObj);
    }
  }
}
