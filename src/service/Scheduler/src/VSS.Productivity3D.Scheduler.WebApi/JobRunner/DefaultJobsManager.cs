using System;
using System.Linq;
using Hangfire.Logging;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  internal class DefaultJobsManager : IDefaultJobRunner
  {
    private readonly IRecurringJobRunner jobRunner;
    private readonly ILogger log;

    public DefaultJobsManager(IRecurringJobRunner jobRunner, ILoggerFactory logger)
    {
      this.jobRunner = jobRunner;
      this.log = logger.CreateLogger<DefaultJobsManager>();
    }

    public void StartDefaultJob(RecurringJobRequest request)
    {
      try
      {
        var existingJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();

        log.LogInformation($"Registered jobs: {existingJobs?.Where(obj => obj.Job.Args.Any(arg => arg.GetType() == typeof(JobRequest)))?.Select((j => j.Job.Method.Name)).Aggregate(string.Empty,(i, j) => i + ';' + j)}");

        if (existingJobs.Where(obj => obj.Job.Args.Any(arg => arg.GetType() == typeof(JobRequest))).Any(obj =>
          ((JobRequest) obj.Job.Args.First(arg => arg.GetType() == typeof(JobRequest))).JobUid == request.JobUid))
        {
          log.LogInformation($"Job with uid {request.JobUid} has been registered - skipping registration.");
          return;
        }

        log.LogInformation($"Instantiating a new job {JsonConvert.SerializeObject(request)}");
        jobRunner.QueueHangfireRecurringJob(request);
      }

      catch (Exception ex)
      {
        log.LogCritical(ex,$"Something wrong wrong with jobs - please delete old recurring jobs");
   //     throw;
      }


    }
  }
}
