using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.DatabaseAgent
{
  public interface ILiteDbAgent
  {
    IEnumerable<T> GetTable<T>(string tableName);
    void DropTables(string[] tableNames);
    void WriteRecord(string tableName, Project project);
    void WriteRecord(string tableName, ImportedFileDescriptor file);
    void WriteError(string projectUid, string errorMessage);
    void SetMigrationState(string tableName, Project project, MigrationState migrationState, string reason);
    void SetMigrationState(string tableName, ImportedFileDescriptor file, MigrationState migrationState);
    void SetFileSize(string tableName, ImportedFileDescriptor file, long length);
    void SetProjectCoordinateSystemDetails(string tableName, Project project);
    void SetProjectDxfUnitsType(string tableName, Project project, DxfUnitsType? dxfUnitsType);
    void SetProjectFilesDetails(string tableName, Project project, int totalFileCount, int eligibleFileCount);
    void SetCanResolveCSIB(string tableName, string key, bool canResolveCsib);
    void SetResolveCSIBMessage(string tableName, string key, string message);
    void SetProjectCSIB(string tableName, string key, string csib);

    void InitDatabase();
    void SetMigationInfo_EndTime();
    void SetMigationInfo_SetProjectCount(int projectCount);
    void SetMigationInfo_IncrementProjectsProcessed();
    void SetMigrationFilesTotal(int fileCount);
    void SetMigrationFilesUploaded(int fileCount);
  }
}
