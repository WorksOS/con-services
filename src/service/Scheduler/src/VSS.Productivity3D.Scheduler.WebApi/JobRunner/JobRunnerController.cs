using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// Handles requests for running VSS jobs
  /// </summary>
  public class JobRunnerController : Controller
  {
    private readonly ILogger Log;
    private readonly IVSSHangfireJobRunner JobRunner;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public JobRunnerController(ILoggerFactory loggerFactory, IVSSHangfireJobRunner jobRunner)
    {
      Log = loggerFactory.CreateLogger<JobRunnerController>();
      JobRunner = jobRunner;
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
        Log.LogError($"Queue VSS job failed with exception {e.Message}");
        throw;
      }

      //Hangfire will substitute a PerformContext automatically
      return new ScheduleJobResult { JobId = hangfireJobId };
    }
  }
}
