using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using TCCToDataOcean.Utils;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.DatabaseAgent
{
  public class LiteDbAgent : ILiteDbAgent, IDisposable
  {
    private readonly LiteDatabase db;

    public LiteDbAgent(IConfigurationStore configurationStore, IEnvironmentHelper environmentHelper)
    {
      var databasePath = Path.Combine(Directory.GetCurrentDirectory(), "database");
      var databaseSuffix = environmentHelper.GetVariable("MIGRATION_TARGET", 1);

      if (!Directory.Exists(databasePath)) { Directory.CreateDirectory(databasePath); }

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

    public T GetRecord<T>(string tableName, int id) where T : MigrationObj => db.GetCollection<T>(tableName).FindById(id);

    public void InitDatabase()
    {
      db.GetCollection<MigrationInfo>(Table.MigrationInfo).Insert(new MigrationInfo());
    }

    public void SetMigationInfo_EndTime()
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.FindById(1);

      var endTimeUtc = DateTime.Now;
      dbObj.EndTime = endTimeUtc;
      dbObj.Duration = endTimeUtc.Subtract(dbObj.StartTime).ToString();
      objs.Update(dbObj);
    }

    public void SetMigationInfo_SetProjectCount(int projectCount)
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.FindById(1);

      dbObj.ProjectsTotal = projectCount;
      objs.Update(dbObj);
    }

    public void SetMigationInfo_IncrementProjectsProcessed()
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.FindById(1);

      dbObj.ProjectsCompleted += 1;
      objs.Update(dbObj);
    }

    public void WriteRecord(string tableName, Project project)
    {
      var objs = db.GetCollection<MigrationProject>(tableName);
      var dbObj = objs.FindById(project.LegacyProjectID);

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

    public void WriteWarning(string projectUid, string message) => db.GetCollection<MigrationMessage>(Table.Warnings).Insert(new MigrationMessage(projectUid, message));
    public void WriteError(string projectUid, string message) => db.GetCollection<MigrationMessage>(Table.Errors).Insert(new MigrationMessage(projectUid, message));

    public void SetMigrationState(string tableName, MigrationJob job, MigrationState migrationState, string message)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == job.Project.ProjectUID);

      dbObj.MigrationState = migrationState;
      dbObj.MigrationStateMessage = message;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void SetMigrationFilesTotal(int fileCount)
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.FindById(1);

      dbObj.FilesTotal += fileCount;
      objs.Update(dbObj);
    }

    public void SetMigrationFilesUploaded(int fileCount)
    {
      var objs = db.GetCollection<MigrationInfo>(Table.MigrationInfo);
      var dbObj = objs.FindById(1);

      dbObj.FilesUploaded += fileCount;
      objs.Update(dbObj);
    }

    public void WriteRecord(string tableName, ImportedFileDescriptor file)
    {
      var objs = db.GetCollection<MigrationFile>(tableName);
      var dbObj = objs.FindById(file.LegacyFileId);

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

    public void SetMigrationState(string tableName, ImportedFileDescriptor file, MigrationState migrationState)
    {
      var files = db.GetCollection<MigrationFile>(tableName);
      var dbObj = files.FindById(file.LegacyFileId);

      dbObj.MigrationState = migrationState;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      files.Update(dbObj);
    }

    public void SetFileSize(string tableName, ImportedFileDescriptor file, long length)
    {
      var files = db.GetCollection<MigrationFile>(tableName);
      var dbObj = files.FindById(file.LegacyFileId);

      dbObj.Length = length;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      files.Update(dbObj);
    }

    public void SetProjectCoordinateSystemDetails(Project project)
    {
      var projects = db.GetCollection<MigrationProject>(Table.Projects);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.DcFilename = project.CoordinateSystemFileName;
      dbObj.HasValidDcFile = !string.IsNullOrEmpty(project.CoordinateSystemFileName);
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void SetProjectDxfUnitsType(string tableName, Project project, DxfUnitsType? dxfUnitsType)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.DxfUnitsType = dxfUnitsType;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void IncrementProjectFilesUploaded(Project project, int fileCount = 1)
    {
      var projects = db.GetCollection<MigrationProject>(Table.Projects);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.UploadedFileCount += fileCount;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void IncrementProjectMigrationCounter(Project project, int count = 1)
    {
      var projects = db.GetCollection<MigrationProject>(Table.Projects);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.MigrationAttempts += count;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
    }

    public void SetProjectFilesDetails(string tableName, Project project, int totalFileCount, int eligibleFileCount)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.TotalFileCount = totalFileCount;
      dbObj.EligibleFileCount = eligibleFileCount;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      projects.Update(dbObj);
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

    public void Dispose()
    {
      db?.Dispose();
    }
  }
}
