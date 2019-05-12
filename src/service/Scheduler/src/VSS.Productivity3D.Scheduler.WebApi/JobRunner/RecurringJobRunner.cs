using System;
using Hangfire;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  public class RecurringJobRunner : JobRunner, IRecurringJobRunner
  {
    public RecurringJobRunner(ILoggerFactory logger, IErrorCodesProvider errorCodesProvider,
      IConfigurationStore configStore, IJobFactory jobFactory, IDevOpsNotification devOpsNotification) : base(
      logger, errorCodesProvider, configStore, jobFactory, devOpsNotification)
    {
    }


    public string QueueHangfireRecurringJob(RecurringJobRequest request)
    {
      string recurringJobId = Guid.NewGuid().ToString();
      RecurringJob.AddOrUpdate(recurringJobId, () => RunHangfireJob(request,null),request.Schedule);
      return recurringJobId;
    }

    public void StopHangfireRecurringJob(string jobUid)
    {
      RecurringJob.RemoveIfExists(jobUid);
    }
  }
}
