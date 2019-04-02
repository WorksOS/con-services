using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IRecurringJobRunner : IJobRunner
  {
    string QueueHangfireRecurringJob(RecurringJobRequest request);
    void StopHangfireRecurringJob(string jobUid);
  }
}