using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockSchedulerController : Controller
  {
    [Route("/internal/v1/mock/export")]
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
    public JobStatusResult GetMockExportJobStatus(string jobId, [FromUri] string filename)
    {
      string status = IN_PROGRESS_STATUS;
      if (jobId == SUCCESS_JOB_ID)
        status = SUCCESS_STATUS;
      else if (jobId == FAILURE_JOB_ID)
        status = FAILURE_STATUS;

      return new JobStatusResult { key = GetS3Key(jobId, filename), status = status };
    }

    private string GetS3Key(string jobId, string filename)
    {
      return $"3dpm/{jobId}/{filename}.zip";
    }

    public static readonly string SUCCESS_JOB_ID = "Test_Job_1";
    private readonly string FAILURE_JOB_ID = "Test_Job_2";
    private readonly string IN_PROGRESS_JOB_ID = "Test_Job_3";

    private readonly string SUCCESS_STATUS = "SUCCEEDED";
    private readonly string FAILURE_STATUS = "FAILED";
    private readonly string IN_PROGRESS_STATUS = "PROCESSING";

  }
}