using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class CompactionExportResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "downloadLink")]
    public string DownloadLink { get; private set; }

    public CompactionExportResult(string downloadLink)
    {
      DownloadLink = downloadLink;
    }

    public CompactionExportResult()
    {

    }

    public CompactionExportResult(int code, string message)
    {
      DownloadLink = string.Empty;
      Code = code;
      Message = message;
    }

  }
}
