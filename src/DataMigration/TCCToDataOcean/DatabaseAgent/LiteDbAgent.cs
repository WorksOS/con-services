using System;
using LiteDB;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.DatabaseAgent
{
  public class LiteDbAgent : ILiteDbAgent
  {
    private readonly LiteDatabase db;

    public LiteDbAgent(IConfigurationStore configurationStore)
    {
      db = new LiteDatabase(configurationStore.GetValueString("LITEDB_MIGRATION_DATABASE"));
    }

    public void DropTables(string[] tableNames)
    {
      foreach (var tablename in tableNames)
      {
        db.DropCollection(tablename);
      }
    }

    public void WriteRecord(string tableName, Project project)
    {
      db.GetCollection<MigrationProject>(tableName).Insert(new MigrationProject(project));
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
      db.GetCollection<MigrationFile>(tableName).Insert(new MigrationFile(file));
    }

    public void SetMigrationState(string tableName, FileData file, MigrationState migrationState)
    {
      var files = db.GetCollection<MigrationFile>(tableName);
      var dbObj = files.FindOne(x => x.Id == file.LegacyFileId);

      dbObj.MigrationState = migrationState;
      dbObj.DateTimeUpdated = DateTime.UtcNow;

      files.Update(dbObj);
    }

    public class MigrationObj
    {
      public int Id { get; set; }
      public DateTime DateTimeCreated { get; set; }
      public DateTime DateTimeUpdated { get; set; }

      public MigrationObj()
      {
        DateTimeCreated = DateTime.UtcNow;
        DateTimeUpdated = DateTimeCreated;
      }
    }

    public class MigrationErrorMessage : MigrationObj
    {
      public string ProjectUid { get; set; }
      public string Error { get; set; }

      public MigrationErrorMessage()
      { }

      public MigrationErrorMessage(string projectUid, string errorMessage)
      {
        ProjectUid = projectUid;
        Error = errorMessage;
      }
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
      public static string Errors = "Errors";
    }
  }
}
