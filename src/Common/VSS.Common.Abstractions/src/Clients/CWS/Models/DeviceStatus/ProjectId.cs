using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  public class ProjectId
  {
    private string _projectTrn;

    [JsonProperty("projectId")]
    public string TRN
    {
      get => _projectTrn;
      set
      {
        _projectTrn = value;
        projectUid = TRNHelper.ExtractGuidAsString(value);
      }
    }

    /// <summary>
    /// WorksOS device ID; the Guid extracted from the TRN.
    /// </summary>
    public string projectUid { get; private set; }
  }
}
