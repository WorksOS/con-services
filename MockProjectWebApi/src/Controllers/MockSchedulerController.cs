using Microsoft.AspNetCore.Mvc;
using System;
using VSS.MasterData.Models.Local.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockSchedulerController : Controller
  {
    [Route("/api/v1/mock/export")]
    [HttpPost]
    public ScheduleJobResult StartMockExport([FromBody] ScheduleJobRequest request)
    {
      var jobId = IN_PROGRESS_JOB_ID;
      if (request.Url.Contains("Test-success"))
        jobId = SUCCESS_JOB_ID;
      if (request.Url.Contains("Test-failed"))
        jobId = FAILURE_JOB_ID;
      return new ScheduleJobResult { jobId = jobId };
    }

    [Route("/api/v1/mock/export/{jobId}")]
    [HttpGet]
    public JobStatusResult GetMockExportJobStatus(string jobId)
    {
      string status = IN_PROGRESS_STATUS;
      if (jobId == SUCCESS_JOB_ID)
        status = SUCCESS_STATUS;
      else if (jobId == FAILURE_JOB_ID)
        status = FAILURE_STATUS;

      return new JobStatusResult { key = jobId, status = status };
    }
    /*
    #region old
    [Route("/api/v1/mock/export/veta")]
    [HttpGet]
    public ScheduleJobResult StartMockVetaExport(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid)
    {
      var jobId = IN_PROGRESS_JOB_ID;
      if (fileName == "Test-success")
        jobId = SUCCESS_JOB_ID;
      if (fileName == "Test-failed")
        jobId = FAILURE_JOB_ID;
      return new ScheduleJobResult{ jobId = jobId};
    }

    [Route("/api/v1/mock/export/veta/{projectUid}/{jobId}")]
    [HttpGet]
    public JobStatusResult GetMockVetaExportJobStatus(Guid projectUid, string jobId)
    {
      string status = IN_PROGRESS_STATUS;
      if (jobId == SUCCESS_JOB_ID)
        status = SUCCESS_STATUS;
      else if (jobId == FAILURE_JOB_ID)
        status = FAILURE_STATUS;

      var customerUid = Request.Headers["X-VisionLink-CustomerUID"];
      var key = $"3dpm/{customerUid}/{projectUid}/{jobId}";

      return new JobStatusResult {key = key, status = status};
    }
#endregion
*/

    public static readonly string SUCCESS_JOB_ID = "Test_Job_1";
    private readonly string FAILURE_JOB_ID = "Test_Job_2";
    private readonly string IN_PROGRESS_JOB_ID = "Test_Job_3";

    private readonly string SUCCESS_STATUS = "SUCCEEDED";
    private readonly string FAILURE_STATUS = "FAILED";
    private readonly string IN_PROGRESS_STATUS = "PROCESSING";

  }
}