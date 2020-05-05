using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class CreateFileRequestModel
  {
    /// <summary>
    /// file name or path?
    /// </summary>
    [JsonProperty("fileName")]
    public string FileName { get; set; }
  }   
}
