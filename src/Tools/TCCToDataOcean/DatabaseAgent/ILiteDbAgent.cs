using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.DatabaseAgent
{
  public interface ILiteDbAgent
  {
    void DropTables(string[] tableNames);
    IEnumerable<T> GetTable<T>(string tableName) where T : MigrationObj;

    T Find<T>(string tableName, int id = -1) where T : MigrationObj;
    IEnumerable<MigrationObj> Find<T>(string tableName, Expression<Func<T, bool>> predicate) where T : MigrationObj;
    long Insert<T>(T obj, string Tablename = null) where T : MigrationObj;
    void Update<T>(long id, Action<T> action, string tableName = null) where T : MigrationObj;
    void WriteRecord(string tableName, ImportedFileDescriptor file);
    void SetMigrationState(MigrationJob job, MigrationState migrationState, string reason);
    void IncrementProjectFilesUploaded(Project project, int fileCount = 1);
    void IncrementProjectMigrationCounter(Project project, int count = 1);
    void SetResolveCSIBMessage(string tableName, string key, string message);
    void SetProjectCSIB(string tableName, string key, string csib);
  }
}
