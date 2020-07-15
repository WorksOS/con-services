using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  public class ProjectId
  {
    private string _projectTrn;

    [JsonProperty("projectId")]
    public string ProjectTrn
    {
      get => _projectTrn;
      set
      {
        _projectTrn = value;
        ProjectUid = TRNHelper.ExtractGuidAsString(value);
      }
    }

    /// <summary>
    /// WorksOS device ID; the Guid extracted from the TRN.
    /// </summary>
    [JsonProperty("projectUid")]
    public string ProjectUid { get; private set; }
  }
}
