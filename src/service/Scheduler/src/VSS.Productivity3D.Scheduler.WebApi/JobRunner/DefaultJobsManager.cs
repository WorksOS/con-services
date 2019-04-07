using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  internal class DefaultJobsManager : IDefaultJobRunner
  {
    private readonly IRecurringJobRunner jobRunner;
    private readonly ILogger<DefaultJobsManager> logger;

    public DefaultJobsManager(IRecurringJobRunner jobRunner, ILogger<DefaultJobsManager> logger)
    {
      this.jobRunner = jobRunner;
      this.logger = logger;
    }

    public void StartDefaultJob(RecurringJobRequest request)
    {
      var existingJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();

      if (existingJobs.Where(obj => obj.Job.Args.Any(arg => arg.GetType() == typeof(JobRequest))).Any(obj =>
        ((JobRequest) obj.Job.Args.First(arg => arg.GetType() == typeof(JobRequest))).JobUid == request.JobUid))
        return;

      jobRunner.QueueHangfireRecurringJob(request);
    }
  }
}
