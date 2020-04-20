using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class CreateFileRequestModel
  {
    /// <summary>
    /// file name or path? todoCCSSSCON-205
    /// </summary>
    [JsonProperty("fileName")]
    public string FileName { get; set; }
  }   
}
