using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class CompactionExportResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "fullFileName")]
    public string FullFileName { get; private set; }

    public CompactionExportResult(string fullFileName)
    {
      FullFileName = fullFileName;
    }
  }
}
