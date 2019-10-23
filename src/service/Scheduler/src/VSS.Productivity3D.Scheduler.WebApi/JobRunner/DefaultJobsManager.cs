using System.Linq;
using Hangfire.Storage;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  internal class DefaultJobsManager : IDefaultJobRunner
  {
    private readonly IRecurringJobRunner jobRunner;

    public DefaultJobsManager(IRecurringJobRunner jobRunner)
    {
      this.jobRunner = jobRunner;
    }

    public void StartDefaultJob(RecurringJobRequest request)
    {
      var existingJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();

      if (existingJobs.Where(obj => obj.Job.Args.Any(arg => arg.GetType() == typeof(JobRequest))).Any(obj =>
         ((JobRequest)obj.Job.Args.First(arg => arg.GetType() == typeof(JobRequest))).JobUid == request.JobUid))
      {
        return;
      }

      jobRunner.QueueHangfireRecurringJob(request);
    }
  }
}
