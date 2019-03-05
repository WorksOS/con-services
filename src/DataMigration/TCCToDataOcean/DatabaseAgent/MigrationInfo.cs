using System;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationInfo : MigrationObj
  {
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Duration { get; set; }
    public int ProjectsTotal { get; set; }
    public int EligibleProjects { get; set; }
    public int ProjectsCompleted { get; set; }
    public int FilesTotal { get; set; }
    public int FilesEligible { get; set; }
    public int Errors { get; set; }

    public MigrationInfo()
    {
      StartTime = DateTime.Now;
    }
  }
}
