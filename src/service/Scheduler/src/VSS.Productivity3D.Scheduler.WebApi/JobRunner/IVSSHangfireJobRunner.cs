using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  public interface IVSSHangfireJobRunner
  {
    string QueueHangfireJob(JobRequest request);
  }
}
