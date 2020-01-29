using System.Collections.Generic;
using System.Net;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.TagFileSplitter.Models
{
  public class TargetServiceResponse : ContractExecutionResult, IMasterDataModel
  {
    public string ApiService;
    public HttpStatusCode StatusCode;

    public TargetServiceResponse(string apiService, int code, string message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      Code = code;
      Message = message;
      ApiService = apiService;
      StatusCode = statusCode;
    }

    public List<string> GetIdentifiers() => new List<string>();
  }
}
