using System;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationProject : MigrationObj
  {
    public string ProjectUid { get; set; }
    public long ProjectId { get; set; }
    public string ProjectName { get; set; }
    public ProjectType ProjectType { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public MigrationState MigrationState { get; set; }
    public bool HasValidDcFile { get; set; }
    public string DcFilename { get; set; }
    public int TotalFileCount { get; set; }
    public int EligibleFileCount { get; set; }

    public MigrationProject()
    { }

    public MigrationProject(Project project)
    {
      Id = project.LegacyProjectID;
      ProjectId = project.LegacyProjectID;
      ProjectUid = project.ProjectUID;
      ProjectName = project.Name;
      ProjectType = project.ProjectType;
      IsDeleted = project.IsDeleted;
      SubscriptionEndDate = project.SubscriptionEndDate;
      DcFilename = project.CoordinateSystemFileName;
    }
  }
}
