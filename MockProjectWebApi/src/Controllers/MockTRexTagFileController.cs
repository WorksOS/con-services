using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockTRexTagFileController : Controller
  {
    /// <summary>
    /// Mock for TRex gateways:   [Route("api/v2/tagfiles")]
    /// </summary>
    [Route("api/v2/mocktrextagfile")]
    [HttpPost]
    public ContractExecutionResult SendTagFileNonDirect([FromBody] CompactionTagFileRequest compactionTagFileRequest)
    {
      Console.WriteLine($"SendTagFileNonDirect: CompactionTagFileRequest {JsonConvert.SerializeObject(compactionTagFileRequest)}");
      return new ContractExecutionResult();
    }

    /// <summary>
    /// Mock for TRex gateways:   [Route("api/v2/tagfiles/direct")]
    /// </summary>
    [Route("api/v2/mocktrextagfile/direct")]
    [HttpPost]
    public ContractExecutionResult SendTagFileDirect([FromBody] CompactionTagFileRequest compactionTagFileRequest)
    {
      Console.WriteLine($"SendTagFileDirect: CompactionTagFileRequest {JsonConvert.SerializeObject(compactionTagFileRequest)}");
      return new ContractExecutionResult();
    }
  }

}
