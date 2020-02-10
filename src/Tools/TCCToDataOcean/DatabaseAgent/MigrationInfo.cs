using System;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationInfo : MigrationObj
  {
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Duration { get; set; }
    public int ProjectsTotal { get; set; }
    public int ProjectsCompleted { get; set; }
    public int FilesTotal { get; set; }
    public int FilesUploaded { get; set; }
    public int ProjectsSuccessful { get; set; }
    public int ProjectsFailed { get; set; }
    public int ProjectsSkipped { get; set; }
    public int ProjectsWithNoFiles { get; set; }
    public int ProjectsWithNoEligibleFiles { get; set; }

    public MigrationInfo()
    {
      TableName = Table.MigrationInfo;
      StartTime = DateTime.Now;
    }
  }
}
