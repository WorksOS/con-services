using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling
{
  public class DrainageResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "PondingFile", Required = Required.Default)]
    public string PondingFile { get; private set; }

    public DrainageResult(string pondingFile)
    {
      PondingFile = pondingFile;
    }
  }
}
