using System;
using System.Linq;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Handles requests for scheduling a long running veta export and getting its progress.
  /// </summary>
  public class ExportController : Controller
  {
    private IVetaExportJob vetaExportJob;

    /// <summary>
    /// Constrictor with dependency injection
    /// </summary>
    public ExportController(IVetaExportJob vetaExport)
    {
      vetaExportJob = vetaExport;
    }
    
    /// <summary>
    /// Schedule a veta export
    /// </summary>
    /// <param name="projectUid">Project for the veta export</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="machineNames">Machine names for the veta export</param>
    /// <param name="filterUid">Filter to apply</param>
    /// <returns></returns>
    [Route("api/v1/export/veta")]
    [HttpGet]
    public string StartVetaExport(
      [FromServices]
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid)
    {

      return BackgroundJob.Enqueue(() => vetaExportJob.ExportDataToVeta(
        projectUid, fileName, machineNames, filterUid, Request.Headers.GetCustomHeaders(true), null));
      //Hangfire will substitute a PerformContext automatically
    }

    /// <summary>
    /// Get the status of a veta export
    /// </summary>
    /// <param name="projectUid">The project Uid</param>
    /// <param name="jobId">The job id</param>
    /// <returns>The AWS S3 key where the file has been saved and the current state of the job</returns>
    [Route("api/v1/export/veta/{projectUid}/{jobId}")]
    [HttpGet]
    public JobStatusResult GetVetaExportJobStatus(Guid projectUid, string jobId)
    {
      var status = JobStorage.Current.GetMonitoringApi().JobDetails(jobId)?.History.LastOrDefault()?.StateName;
      if (string.IsNullOrEmpty(status))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Missing job details for {jobId}"));
      }
      return new JobStatusResult{key = VetaExportJob.GetS3Key(projectUid, jobId), status = status};
    }

  }
}
