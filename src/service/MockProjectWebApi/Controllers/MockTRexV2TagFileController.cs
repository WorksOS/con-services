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
    public ContractExecutionResult SendTagFileNonDirect([FromBody] CompactionTagFileRequest compactionTagFileRequest)
    {
      Logger.LogInformation($"SendTagFileNonDirect: CompactionTagFileRequest {JsonConvert.SerializeObject(compactionTagFileRequest)}");
      return new ContractExecutionResult();
    }

    [Route("api/v2/direct")]
    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public ContractExecutionResult SendTagFileDirect([FromBody] CompactionTagFileRequest compactionTagFileRequest)
    {
      Logger.LogInformation($"SendTagFileDirect: CompactionTagFileRequest {JsonConvert.SerializeObject(compactionTagFileRequest)}");
      return new ContractExecutionResult();
    }
  }
}
