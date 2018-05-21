using System.Net;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;

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
      if (request.Url.Contains("Test-timeout"))
        jobId = TIMEOUT_JOB_ID;
      return new ScheduleJobResult { JobId = jobId };
    }

    //I don't think this is called. It's done through the mick veta export in MockRaptorController
    [Route("/api/v1/mock/export/{jobId}")]
    [HttpGet]
    public JobStatusResult GetMockExportJobStatus(string jobId)
    {
      string downloadLink = null;
      FailureDetails details = null;

      string status = IN_PROGRESS_STATUS;
      if (jobId == SUCCESS_JOB_ID)
      {
        status = SUCCESS_STATUS;
        downloadLink = "some download link";
      }
      else if (jobId == FAILURE_JOB_ID)
      {
        status = FAILURE_STATUS;
        details = new FailureDetails {Code = HttpStatusCode.BadRequest, Result = new ContractExecutionResult(2005, "Export limit reached") };
      }

      return new JobStatusResult { Key = GetS3Key(jobId, "dummy file"), Status = status, DownloadLink= downloadLink, FailureDetails = details };
    }

    private string GetS3Key(string jobId, string filename)
    {
      return $"3dpm/{jobId}/{filename}.zip";
    }

    public static readonly string SUCCESS_JOB_ID = "Test_Job_1";
    public static readonly string FAILURE_JOB_ID = "Test_Job_2";
    private readonly string IN_PROGRESS_JOB_ID = "Test_Job_3";
    public static readonly string TIMEOUT_JOB_ID = "Test_Job_4";

    private readonly string SUCCESS_STATUS = "SUCCEEDED";
    private readonly string FAILURE_STATUS = "FAILED";
    private readonly string IN_PROGRESS_STATUS = "PROCESSING";

  }
}