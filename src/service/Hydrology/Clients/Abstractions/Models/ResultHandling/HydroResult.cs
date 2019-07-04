using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling
{
  public class HydroResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "fullFileName")]
    public string FullFileName { get; private set; }

    public HydroResult(string fullFileName)
    {
      FullFileName = fullFileName;
    }
  }
}
