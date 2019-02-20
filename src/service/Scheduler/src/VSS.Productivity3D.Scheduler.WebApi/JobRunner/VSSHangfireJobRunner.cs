using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// Runs VSS jobs using Hangfire
  /// </summary>
  public class VSSHangfireJobRunner : IVSSJobRunner, IVSSHangfireJobRunner
  {
    private readonly ILogger Log;
    private readonly SchedulerErrorCodesProvider ErrorCodesProvider;
    private readonly IServiceProvider ServiceProvider;
    private Dictionary<Guid, Type> vssJobs;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public VSSHangfireJobRunner(ILoggerFactory logger, IErrorCodesProvider errorCodesProvider, IServiceProvider serviceProvider)
    {
      Log = logger.CreateLogger<VSSHangfireJobRunner>();
      ErrorCodesProvider = errorCodesProvider as SchedulerErrorCodesProvider;
      ServiceProvider = serviceProvider;
      vssJobs = new Dictionary<Guid, Type>();
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
    [AutomaticRetry(Attempts = 3)]
    //[AutomaticRetry(Attempts = 3, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    //[DisableConcurrentExecution(5)]
    public Task<ContractExecutionResult> RunHangfireJob(JobRequest request, PerformContext context)
    {
      return RunJob(request);
    }

    /// <summary>
    /// Runs a VSS job
    /// </summary>
    public async Task<ContractExecutionResult> RunJob(JobRequest request)
    {
      //TODO for real
      //Find the job with give job id in our list of jobs
      if (!vssJobs.ContainsKey(request.JobUid))
      {
        return ErrorCodesProvider.CreateErrorResult(SchedulerErrorCodes.MissingVSSJob, request.JobUid.ToString());
      }
      Log.LogInformation($"Found job {request.JobUid}");
      IVSSJob job = null;
      try
      {
        job = ActivatorUtilities.GetServiceOrCreateInstance(ServiceProvider, vssJobs[request.JobUid]) as IVSSJob;
        if (job == null)
        {
          Log.LogError($"Failed to create VSS job {request.JobUid}: job is null");
          return ErrorCodesProvider.CreateErrorResult(SchedulerErrorCodes.VSSJobCreationFailure, request.JobUid.ToString(), "job is null");
        }
      }
      catch (Exception e)
      {
        Log.LogError($"Failed to create VSS job {request.JobUid}: {e.Message}");
        return ErrorCodesProvider.CreateErrorResult(SchedulerErrorCodes.VSSJobCreationFailure, request.JobUid.ToString(), e.Message);
      }
 
      try
      {
        Log.LogInformation($"Setting up job {request.JobUid}");
        await job.Setup(request.SetupParameters);
        Log.LogInformation($"Running job {request.JobUid}");
        await job.Run(request.RunParameters);
        Log.LogInformation($"Tearing down job {request.JobUid}");
        await job.TearDown(request.TearDownParameters);
        Log.LogInformation($"Job {request.JobUid} completed");
        return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully);
      }
      catch (Exception e)
      {
        Log.LogError($"Failed to execute VSS job {request.JobUid}: {e.Message}");
        return ErrorCodesProvider.CreateErrorResult(SchedulerErrorCodes.VSSJobExecutionFailure, request.JobUid.ToString(), e.Message);
      }
    }

    /// <summary>
    /// Register a VSS job.
    /// </summary>
    /// <param name="uid">Unique id for the job</param>
    /// <param name="type">The type of the job for instantiation</param>
    public void RegisterJob(Guid uid, Type type)
    {
      if (vssJobs.ContainsKey(uid))
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"Job {uid} is already registered"));
      }
      vssJobs.Add(uid, type);
    }

  }
}
