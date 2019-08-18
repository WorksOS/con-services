using System;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
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
    public string MigrationStateMessage { get; set; }
    public bool HasValidDcFile { get; set; }
    public string DcFilename { get; set; }
    public DxfUnitsType? DxfUnitsType { get; set; }
    public int TotalFileCount { get; set; }
    public int EligibleFileCount { get; set; }
    public bool CanResolveCSIB { get; set; }
    public string ResolveCSIBMessage { get; set; }
    public string CSIB { get; set; }

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
