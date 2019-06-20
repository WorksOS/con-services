using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models
{
  public class DrainageRequest
  {
    [JsonProperty(PropertyName = "DesignFileName", Required = Required.Default)]
    public string DesignFileName { get; protected set; }

    public DrainageRequest(string designFileName)
    {
      DesignFileName = designFileName;
    }

    public void Validate()
    {
      if (string.IsNullOrEmpty(DesignFileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "A designFileName must be provided"));
      }
    }
  }
}

