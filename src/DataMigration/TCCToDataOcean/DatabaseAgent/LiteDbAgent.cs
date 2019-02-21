﻿using LiteDB;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.DatabaseAgent
{
  public interface ILiteDbAgent
  {
    void DropTables(string[] tableNames);
    void WriteRecord(string tableName, Project project);
    void WriteRecord(string tableName, FileData file);
    void SetMigrationState(string tableName, Project project, LiteDbAgent.MigrationState migrationState);
    void SetMigrationState(string tableName, FileData file, LiteDbAgent.MigrationState migrationState);
  }

  public class LiteDbAgent : ILiteDbAgent
  {
    private ILogger _log;
    private readonly string _databaseName;

    public LiteDbAgent(ILoggerFactory loggerFactory, IConfigurationStore configurationStore)
    {
      _log = loggerFactory.CreateLogger(GetType());

      _databaseName = configurationStore.GetValueString("LITEDB_MIGRATION_DATABASE");
    }

    public void DropTables(string[] tableNames)
    {
      using (var db = new LiteDatabase(_databaseName))
      {
        foreach (var tablename in tableNames)
        {
          db.DropCollection(tablename);
        }
      }
    }

    public void WriteRecord(string tableName, Project project)
    {
      using (var db = new LiteDatabase(_databaseName))
      {
        db.GetCollection<MigrationProject>(tableName).Insert(new MigrationProject(project));
      }
    }

    public void SetMigrationState(string tableName, Project project, MigrationState migrationState)
    {
      using (var db = new LiteDatabase(_databaseName))
      {
        var projects = db.GetCollection<MigrationProject>(tableName);
        var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

        dbObj.MigrationState = migrationState;

        projects.Update(dbObj);
      }
    }

    public void WriteRecord(string tableName, FileData file)
    {
      using (var db = new LiteDatabase(_databaseName))
      {
        db.GetCollection<MigrationFile>(tableName).Insert(new MigrationFile(file));
      }
    }

    public void SetMigrationState(string tableName, FileData file, MigrationState migrationState)
    {
      using (var db = new LiteDatabase(_databaseName))
      {
        var files = db.GetCollection<MigrationFile>(tableName);
        var dbObj = files.FindOne(x => x.Id == file.LegacyFileId);

        dbObj.MigrationState = migrationState;

        files.Update(dbObj);
      }
    }


    public class MigrationObj
    {
      public int Id { get; set; }
    }

    public class MigrationProject : MigrationObj
    {
      public string ProjectUid { get; set; }
      public long ProjectId { get; set; }
      public MigrationState MigrationState { get; set; }

      public MigrationProject()
      { }

      public MigrationProject(Project project)
      {
        Id = project.LegacyProjectID;
        ProjectId = project.LegacyProjectID;
        ProjectUid = project.ProjectUID;
      }
    }

    public class MigrationFile : MigrationObj
    {
      public string ProjectUid { get; set; }
      public string ImportedFileUid { get; set; }
      public string CustomerUid { get; set; }
      public ImportedFileType ImportedFileType { get; set; }
      public string Filename { get; set; }
      public MigrationState MigrationState { get; set; }

      public MigrationFile()
      { }

      public MigrationFile(FileData file)
      {
        Id = (int)file.LegacyFileId;
        ProjectUid = file.ProjectUid;
        ImportedFileType = file.ImportedFileType;
        CustomerUid = file.CustomerUid;
        ImportedFileUid = file.ImportedFileUid;
        Filename = file.Name;
      }
    }

    public enum MigrationState
    {
      Unknown,
      Pending,
      InProgress,
      Completed,
      Failed
    }

    public class Table
    {
      public static string Projects = "Projects";
      public static string Files = "Files";
    }
  }

}
