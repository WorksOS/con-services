using System;
using LiteDB;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;

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

    public void SetProjectCoordinateSystemDetails(string tableName, Project project, bool isValid)
    {
      var projects = db.GetCollection<MigrationProject>(tableName);
      var dbObj = projects.FindOne(x => x.ProjectUid == project.ProjectUID);

      dbObj.DcFilename = project.CoordinateSystemFileName;
      dbObj.HasValidDcFile = isValid;
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
  }
}
