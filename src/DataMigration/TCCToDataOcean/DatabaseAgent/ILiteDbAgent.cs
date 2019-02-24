using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;

namespace TCCToDataOcean.DatabaseAgent
{
  public interface ILiteDbAgent
  {
    void DropTables(string[] tableNames);
    void WriteRecord(string tableName, Project project);
    void WriteRecord(string tableName, FileData file);
    void WriteError(string projectUid, string errorMessage);
    void SetMigrationState(string tableName, Project project, LiteDbAgent.MigrationState migrationState);
    void SetMigrationState(string tableName, FileData file, LiteDbAgent.MigrationState migrationState);
  }
}
