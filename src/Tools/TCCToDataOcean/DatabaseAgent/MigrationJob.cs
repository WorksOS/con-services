using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationJob
  {
    public Project Project { get; set; }
    public bool IsRetryAttempt { get; set; }
    public byte[] CoordinateSystemFileBytes { get; set; }
  }
}
