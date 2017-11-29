using Microsoft.AspNetCore.Mvc;
using System;

namespace MockProjectWebApi.Controllers
{
  public class MockSchedulerContoller : Controller
  {
    [Route("/api/v1/export/veta")]
    [HttpGet]
    public string StartVetaJob(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid)
    {
      return "Test_Job_1";
    }

    [Route("/api/v1/export/veta/{projectUId}/{jobId}")]
    [HttpGet]
    public Tuple<string, string> GetVetaExportStatus(Guid projectUid, string jobId)
    {
      return new Tuple<string, string>($"{projectUid.ToString()}/{jobId}", "Succeeded ");
    }
  }
}