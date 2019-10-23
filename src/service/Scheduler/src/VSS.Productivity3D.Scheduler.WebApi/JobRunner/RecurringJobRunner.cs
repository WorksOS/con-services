using System;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  public class RecurringJobRunner : JobRunner, IRecurringJobRunner
  {
    public const string QUEUE_NAME = "recurring";

    public RecurringJobRunner(ILoggerFactory logger, IErrorCodesProvider errorCodesProvider,
      IConfigurationStore configStore, IJobFactory jobFactory, IJobRegistrationManager jobManager, IServiceProvider provider ) : base(
      logger, errorCodesProvider, configStore, jobFactory, jobManager, provider)
    { }


    public string QueueHangfireRecurringJob(RecurringJobRequest request)
    {
      var recurringJobId = Guid.NewGuid().ToString();
      try
      {
        if (!string.IsNullOrEmpty(request.Schedule))
        {
          request.Validate();
          log.LogDebug($"Job request validated, starting a new job {JsonConvert.SerializeObject(request)}");
          RecurringJob.AddOrUpdate(recurringJobId, () => RunHangfireJob(request, false, null, null), request.Schedule, queue: QUEUE_NAME);
        }
      }
      catch (Exception ex)
      {
        log.LogError(ex, $"Can't start scheduled job as the request is invalid");
        return "";
      }
      return recurringJobId;
    }

    public void StopHangfireRecurringJob(string jobUid)
    {
      RecurringJob.RemoveIfExists(jobUid);
    }
  }
}
