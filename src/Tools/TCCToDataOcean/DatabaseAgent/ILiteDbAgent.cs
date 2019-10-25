using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.DatabaseAgent
{
  public interface ILiteDbAgent
  {
    void DropTables(string[] tableNames);
    IEnumerable<T> GetTable<T>(string tableName) where T : MigrationObj;

    T GetRecord<T>(string tableName, int id) where T : MigrationObj;
    IEnumerable<MigrationObj> Find<T>(string tableName, Expression<Func<T, bool>> predicate) where T : MigrationObj;
    bool Insert<T>(T obj, string Tablename = null) where T : MigrationObj;
    void Update<T>(int id, Action<T> action) where T : MigrationObj;
    void WriteRecord(string tableName, Project project);
    void WriteRecord(string tableName, ImportedFileDescriptor file);
    void SetMigrationState(string tableName, MigrationJob job, MigrationState migrationState, string reason);
    void SetMigrationState(string tableName, ImportedFileDescriptor file, MigrationState migrationState);
    void SetFileSize(string tableName, ImportedFileDescriptor file, long length);
    void SetProjectCoordinateSystemDetails(Project project);
    void SetProjectDxfUnitsType(string tableName, Project project, DxfUnitsType? dxfUnitsType);
    void IncrementProjectFilesUploaded(Project project, int fileCount = 1);
    void IncrementProjectMigrationCounter(Project project, int count = 1);
    void SetProjectFilesDetails(string tableName, Project project, int totalFileCount, int eligibleFileCount);
    void SetResolveCSIBMessage(string tableName, string key, string message);
    void SetProjectCSIB(string tableName, string key, string csib);

    void InitDatabase();
    void SetMigationInfo_SetProjectCount(int projectCount);
    void SetMigationInfo_IncrementProjectsProcessed();
    void SetMigrationFilesTotal(int fileCount);
    void SetMigrationFilesUploaded(int fileCount);
  }
}
