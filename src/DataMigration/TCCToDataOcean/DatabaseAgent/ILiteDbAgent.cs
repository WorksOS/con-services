using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.DatabaseAgent
{
  public interface ILiteDbAgent
  {
    IEnumerable<T> GetTable<T>(string tableName);
    void DropTables(string[] tableNames);
    void WriteRecord(string tableName, Project project);
    void WriteRecord(string tableName, FileData file);
    void WriteError(string projectUid, string errorMessage);
    void SetMigrationState(string tableName, Project project, MigrationState migrationState);
    void SetMigrationState(string tableName, FileData file, MigrationState migrationState);
    void SetFileSize(string tableName, FileData file, long length);
    void SetProjectCoordinateSystemDetails(string tableName, Project project, bool isValid);
    void SetProjectFilesDetails(string tableName, Project project, int totalFileCount, int eligibleFileCount);
    void SetCanResolveCSIB(string tableName, string key, bool canResolveCsib);
    void SetResolveCSIBMessage(string tableName, string key, string message);
    void SetProjectCSIB(string tableName, string key, string csib);

    void InitDatabase();
    void SetMigationInfo_EndTime();
    void SetMigationInfo_SetProjectCount(int projectCount);
    void SetMigationInfo_SetEligibleProjectCount(int projectCount);
    void SetMigationInfo_IncrementProjectsProcessed();
  }
}
