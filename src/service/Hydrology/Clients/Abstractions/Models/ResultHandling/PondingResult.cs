using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling
{
  public class PondingResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "fullFileName")]
    public string FullFileName { get; private set; }

    public PondingResult(string fullFileName)
    {
      FullFileName = fullFileName;
    }
  }
}
