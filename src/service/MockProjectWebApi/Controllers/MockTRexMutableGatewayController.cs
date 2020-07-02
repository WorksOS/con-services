using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;

namespace MockProjectWebApi.Controllers
{
  public class MockTRexMutableGatewayController : BaseController
  {
    public MockTRexMutableGatewayController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [HttpPost]
    [Route("api/v1/design")]
    public ContractExecutionResult CreateDesign([FromBody] DesignRequest designRequest)
    {
      return new ContractExecutionResult();
    }

    [HttpPut]
    [Route("api/v1/design")]
    public ContractExecutionResult UpdateDesign([FromBody] DesignRequest designRequest)
    {
      return new ContractExecutionResult();
    }

    [HttpDelete]
    [Route("api/v1/design")]
    public ContractExecutionResult DeleteDesign([FromBody] DesignRequest designRequest)
    {
      return new ContractExecutionResult();
    }
  }
}
