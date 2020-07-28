using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockTRexV2TagFileController : BaseController
  {
    public MockTRexV2TagFileController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v2")]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public ContractExecutionResult SendTagFile([FromBody] CompactionTagFileRequest compactionTagFileRequest)
    {
      Logger.LogInformation($"{nameof(SendTagFile)}: CompactionTagFileRequest {JsonConvert.SerializeObject(compactionTagFileRequest)}");
      return new ContractExecutionResult();
    }
  }
}
