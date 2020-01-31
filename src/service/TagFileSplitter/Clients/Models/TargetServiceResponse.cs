using System.Collections.Generic;
using System.Net;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.TagFileSplitter.Models
{
  public class TargetServiceResponse : ContractExecutionResult, IMasterDataModel
  {
    // note that the VSS one IS significant to tagFileHarvester, and MUST be == ServiceNameConstants.PRODUCTIVITY3D_VSS_SERVICE
    public string ServiceName;
    public HttpStatusCode StatusCode;

    public TargetServiceResponse(string serviceName, int code, string message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      Code = code;
      Message = message;
      ServiceName = serviceName;
      StatusCode = statusCode;
    }

    public List<string> GetIdentifiers() => new List<string>();
  }
}
