using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.DatabaseAgent
{
  public interface ILiteDbAgent
  {
    void DropTables(string[] tableNames);
    void WriteRecord(string tableName, Project project);
    void WriteRecord(string tableName, FileData file);
    void WriteError(string projectUid, string errorMessage);
    void SetMigrationState(string tableName, Project project, MigrationState migrationState);
    void SetMigrationState(string tableName, FileData file, MigrationState migrationState);
    void SetProjectCoordinateSystemDetails(string tableName, Project project, bool isValid);
    void SetProjectFilesDetails(string tableName, Project project, int totalFileCount, int eligibleFileCount);
    void SetCanResolveCSIB(string tableName, string key, bool canResolveCsib);
    void SetProjectCSIB(string tableName, string key, string csib);

    void InitDatabase();
    void SetMigationInfo_EndTime();
    void SetMigationInfo_SetProjectCount(int projectCount);
    void SetMigationInfo_SetEligibleProjectCount(int projectCount);
    void SetMigationInfo_IncrementProjectsProcessed();
  }
}
