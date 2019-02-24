using VSS.MasterData.Repositories.DBModels;

namespace TCCToDataOcean.DatabaseAgent
{
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
}
