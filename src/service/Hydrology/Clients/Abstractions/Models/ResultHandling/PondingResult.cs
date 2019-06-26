using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling
{
  public class PondingResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "PondingFile", Required = Required.Default)]
    public string PondingFile { get; private set; }

    public PondingResult(string pondingFile)
    {
      PondingFile = pondingFile;
    }
  }
}
