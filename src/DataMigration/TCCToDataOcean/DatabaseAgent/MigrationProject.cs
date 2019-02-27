using VSS.MasterData.Repositories.DBModels;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationProject : MigrationObj
  {
    public string ProjectUid { get; set; }
    public long ProjectId { get; set; }
    public string ProjectName { get; set; }
    public MigrationState MigrationState { get; set; }
    public bool HasValidDcFile { get; set; }
    public string DcFilename { get; set; }
    public int TotalFileCoutn { get;set; }
    public int EligibleFileCount { get; set; }

    public MigrationProject()
    { }

    public MigrationProject(Project project)
    {
      Id = project.LegacyProjectID;
      ProjectId = project.LegacyProjectID;
      ProjectUid = project.ProjectUID;
      ProjectName = project.Name;
      DcFilename = project.CoordinateSystemFileName;
    }
  }
}
