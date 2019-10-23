using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Productivity3D.Scheduler.WebApi.JobRunner;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{

  /// <summary>
  /// Runs VSS jobs using Hangfire
  /// </summary>
  public class JobRunner : IJobRunner
  {
    protected readonly ILogger log;
    private readonly SchedulerErrorCodesProvider errorCodesProvider;

    private readonly IJobFactory jobFactory;
    private readonly string environment;
    private readonly IJobRegistrationManager jobManager;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public JobRunner(ILoggerFactory logger, IErrorCodesProvider errorCodesProvider,
      IConfigurationStore configStore, IJobFactory jobFactory, IJobRegistrationManager jobManager, IServiceProvider serviceProvider)
    {
      log = logger.CreateLogger<JobRunner>();
      this.errorCodesProvider = errorCodesProvider as SchedulerErrorCodesProvider;
      this.jobFactory = jobFactory;
      environment = configStore.GetValueString("ENVIRONMENT");
      this.jobManager = jobManager;
      this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Queues a VSS job to be run by hangfire in the background
    /// </summary>
    public string QueueHangfireJob(JobRequest request, IJobCallback onContinuation = null, string continuationJobname = "Default continuation job")
    {
      // We have to pass in a null PerformContext, as Hangfire will inject the correct one when the job is run.

      var client = new BackgroundJobClient();
      var state = new EnqueuedState(jobManager.GetQueueName(request.JobUid));

      //TODO: Ugly code but can't figure out a better way with Hangfire as can't apply dynamic filters on methods in a singleton context
      var parentJob = string.Empty;

      if (request.AttributeFilters == SpecialFilters.ExportFilter)
      {
        parentJob = client.Create(() => RunHangfireJobExportFilter(request, false, null, null), state);
        if (onContinuation != null)
        {
          JobStorage.Current.GetConnection().SetJobParameter(parentJob, Tags.CONTINUATION_TYPE, onContinuation.GetType().FullName);
          return client.ContinueJobWith(parentJob, () => RunHangfireJobExportFilter(
                                          null,
                                          true,
                                          parentJob,
                                          null), nextState: state, options: JobContinuationOptions.OnAnyFinishedState);
        }
      }
      else
      {
        parentJob = client.Create(() => RunHangfireJob(request, false, null, null), state);
        if (onContinuation != null)
        {
          JobStorage.Current.GetConnection().SetJobParameter(parentJob, Tags.CONTINUATION_TYPE, onContinuation.GetType().FullName);
          return client.ContinueJobWith(parentJob, () => RunHangfireJob(
                                          null,
                                          true,
                                          parentJob,
                                          null), nextState: state, options: JobContinuationOptions.OnAnyFinishedState);
        }
      }
      return parentJob;
    }

    /// <summary>
    /// Runs a VSS job through hangfire
    /// </summary>
    /// The filter ensures thath the failed job is executed on the same queue as the original one
    [AutomaticRetry(Attempts = 3)]
    [JobDisplayName("Job: {0}")]
    public Task<ContractExecutionResult> RunHangfireJob(JobRequest request,
                                                        bool onContinuation,
                                                        string parentJobId,
                                                        PerformContext context)
    {
      var internalRequest = request;
      if (onContinuation)
      {
        var jobClassType = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(x => x.GetTypes())
        .Where(x => x.FullName == JobStorage.Current.GetConnection().GetJobParameter(parentJobId, Tags.CONTINUATION_TYPE) && !x.IsInterface && !x.IsAbstract)
        .FirstOrDefault();

        var callback = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, jobClassType) as IJobCallback;
        internalRequest = callback.ExecuteCallback(parentJobId, context);
      }
      internalRequest.Validate();
      return RunJob(internalRequest, context);
    }

    [ExportFailureFilter]
    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [JobDisplayName("Job: {0}")]
    // [MaximumConcurrentExecutions(3, 1200)]
    public Task<ContractExecutionResult> RunHangfireJobExportFilter(JobRequest request,
                                                                    bool onContinuation,
                                                                    string parentJobId,
                                                                    PerformContext context)
    {
      var internalRequest = request;
      if (onContinuation)
      {
        var jobClassType = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(x => x.GetTypes())
        .Where(x => x.FullName == JobStorage.Current.GetConnection().GetJobParameter(parentJobId, Tags.CONTINUATION_TYPE) && !x.IsInterface && !x.IsAbstract)
        .FirstOrDefault();

        var callback = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, jobClassType) as IJobCallback;
        internalRequest = callback.ExecuteCallback(parentJobId, context);
      }
      internalRequest.Validate();

      return RunJob(internalRequest, context);
    }

    [JobFailureFilter]
    [AutomaticRetry(Attempts = 3)]
    [Obsolete("Delete old recurring jobs")]
    public Task<ContractExecutionResult> RunHangfireJob(JobRequest request, PerformContext context)
    {
      request.Validate();

      return RunJob(request, context);
    }

    /// <summary>
    /// Runs a VSS job
    /// </summary>
    protected async Task<ContractExecutionResult> RunJob(JobRequest request, PerformContext context)
    {
      var result = new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully);
      log.LogDebug($"About to run job with request {JsonConvert.SerializeObject(request)}");
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
          await job.Setup(request.SetupParameters, context);
          log.LogInformation($"Running job {request.JobUid}");
          await job.Run(request.RunParameters, context);
          log.LogInformation($"Tearing down job {request.JobUid}");
          await job.TearDown(request.TearDownParameters, context);
          log.LogInformation($"Job {request.JobUid} completed");
        }
        catch (ServiceException se)
        {
          log.LogError(se, "ServiceException error during RunJob():");
          result = errorCodesProvider.CreateErrorResult(environment, SchedulerErrorCodes.VSSJobExecutionFailure,
            request.JobUid.ToString(), se.GetFullContent);
        }
        catch (Exception e)
        {
          log.LogError(e, "Exception error during RunJob():");

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

      log.LogInformation($"{nameof(RunJob)} completed with result: {JsonConvert.SerializeObject(result)}");
      return result;
    }
  }
}

