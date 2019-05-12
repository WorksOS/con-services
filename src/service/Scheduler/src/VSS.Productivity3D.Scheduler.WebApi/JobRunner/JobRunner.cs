﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Productivity3D.Scheduler.WebApi.JobRunner;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  public class JobRunnerHealthCheck : HealthCheck
  {
    public JobRunnerHealthCheck() : base(nameof(JobRunnerHealthCheck))
    {
    }

    protected override ValueTask<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = new CancellationToken())
    {
      return new ValueTask<HealthCheckResult>(State ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
    }

    public static volatile bool State = true;
  }

  /// <summary>
  /// Runs VSS jobs using Hangfire
  /// </summary>
  public class JobRunner : IJobRunner
  {
    private readonly ILogger log;
    private readonly SchedulerErrorCodesProvider errorCodesProvider;

    private readonly IJobFactory jobFactory;
    private readonly string environment;
    private readonly IDevOpsNotification devOpsNotification;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public JobRunner(ILoggerFactory logger, IErrorCodesProvider errorCodesProvider,
      IConfigurationStore configStore, IJobFactory jobFactory, IDevOpsNotification devOpsNotification)
    {
      log = logger.CreateLogger<JobRunner>();
      this.errorCodesProvider = errorCodesProvider as SchedulerErrorCodesProvider;
      this.jobFactory = jobFactory;
      environment = configStore.GetValueString("ENVIRONMENT");
      this.devOpsNotification = devOpsNotification;
    }

    /// <summary>
    /// Queues a VSS job to be run by hangfire in the background
    /// </summary>
    public string QueueHangfireJob(JobRequest request)
    {
      // We have to pass in a null PerformContext, as Hangfire will inject the correct one when the job is run.
      return BackgroundJob.Enqueue(() => RunHangfireJob(request, null));
    }


    /// <summary>
    /// Runs a VSS job through hangfire
    /// </summary>
    [JobFailureFilter]
    [AutomaticRetry(Attempts = 3)]
    public async Task<ContractExecutionResult> RunHangfireJob(JobRequest request, PerformContext context)
    {
      ContractExecutionResult result;
      try
      {
        result = await RunJob(request);
      }
      catch (Exception e)
      {
        devOpsNotification.Notify(e.JobNotificationDetails(context.BackgroundJob.Id));
        throw;
      }
      return result;
    }

    /// <summary>
    /// Runs a VSS job
    /// </summary>
    protected async Task<ContractExecutionResult> RunJob(JobRequest request)
    {
      var result = new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully);

      var job = jobFactory.GetJob(request.JobUid);

      if (job == null)
      {
        result = errorCodesProvider.CreateErrorResult(environment, SchedulerErrorCodes.VSSJobCreationFailure, request.JobUid.ToString(), "Unable to create job");
      }
      else
      {
        try
        {
          log.LogInformation($"Setting up job {request.JobUid}");
          await job.Setup(request.SetupParameters);
          log.LogInformation($"Running job {request.JobUid}");
          await job.Run(request.RunParameters);
          log.LogInformation($"Tearing down job {request.JobUid}");
          await job.TearDown(request.TearDownParameters);
          log.LogInformation($"Job {request.JobUid} completed");
        }
        catch (ServiceException se)
        {
          result = errorCodesProvider.CreateErrorResult(environment, SchedulerErrorCodes.VSSJobExecutionFailure,
            request.JobUid.ToString(), se.GetFullContent);
        }
        catch (Exception e)
        {
          result = errorCodesProvider.CreateErrorResult(environment, SchedulerErrorCodes.VSSJobExecutionFailure,
            request.JobUid.ToString(), e.Message);
          JobRunnerHealthCheck.State = false;
        }
      }

      if (result.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        log.LogError(result.Message);
        throw new ServiceException(HttpStatusCode.InternalServerError, result);
      }

      JobRunnerHealthCheck.State = true;
      return result;
    }
  }
}
  
