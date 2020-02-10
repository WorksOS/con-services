using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// Handles requests for running VSS jobs
  /// </summary>
  public class JobRunnerController : Controller
  {
    private readonly ILogger Log;
    private readonly IJobRunner JobRunner;
    private readonly IRecurringJobRunner RecurringJobRunner;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public JobRunnerController(ILoggerFactory loggerFactory, IJobRunner jobRunner, IRecurringJobRunner recurringJobRunner)
    {
      Log = loggerFactory.CreateLogger<JobRunnerController>();
      JobRunner = jobRunner;
      RecurringJobRunner = recurringJobRunner;
    }

    /// <summary>
    /// Runs the VSS job identified by job Uid with given parameters
    /// </summary>
    [Route("internal/v1/runjob")]
    [HttpPost]
    public ScheduleJobResult RunJob([FromBody] JobRequest request)
    {
      Log.LogInformation($"{nameof(RunJob)}: {JsonConvert.SerializeObject(request)}");
      request.Validate();
      string hangfireJobId;
      try
      {
        hangfireJobId = JobRunner.QueueHangfireJob(request);
      }
      catch (Exception e)
      {
        Log.LogError(e,$"Queue VSS job failed with exception {e.Message}");
        throw;
      }

      //Hangfire will substitute a PerformContext automatically
      return new ScheduleJobResult { JobId = hangfireJobId };
    }

    /// <summary>
    /// Runs the VSS job identified by job Uid with given parameters
    /// </summary>
    [Route("internal/v1/recurring/startjob")]
    [HttpPost]
    public ScheduleJobResult RunRecurringJob([FromBody] RecurringJobRequest request)
    {
      Log.LogInformation($"{nameof(RunRecurringJob)}: {JsonConvert.SerializeObject(request)}");
      request.Validate();
      string hangfireJobId;
      try
      {
        hangfireJobId = RecurringJobRunner.QueueHangfireRecurringJob(request);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Queue VSS job failed with exception {e.Message}");
        throw;
      }

      //Hangfire will substitute a PerformContext automatically
      return new ScheduleJobResult { JobId = hangfireJobId };
    }

    //Todo
    /// <summary>
    /// Runs the VSS job identified by job Uid with given parameters
    /// </summary>
    [Route("internal/v1/recurring/stopjob/{jobId}")]
    [HttpGet]
    public ScheduleJobResult StopRecurringJob([FromRoute] string jobId)
    {
      Log.LogInformation($"{nameof(StopRecurringJob)}: {jobId}");

      try
      {
        RecurringJobRunner.StopHangfireRecurringJob(jobId);
      }
      catch (Exception e)
      {
        Log.LogError(e,$"Queue VSS job failed with exception {e.Message}");
        throw;
      }

      //Hangfire will substitute a PerformContext automatically
      return new ScheduleJobResult { JobId = jobId };
    }
  }
}
