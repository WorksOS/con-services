using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class CompactionExportResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "fullFileName")]
    public string FullFileName { get; private set; }

    private CompactionExportResult()
    {
    }

    public static CompactionExportResult Create(string fullFileName)
    {
      return new CompactionExportResult
      {
        FullFileName = fullFileName
      };
    }
  }
}
