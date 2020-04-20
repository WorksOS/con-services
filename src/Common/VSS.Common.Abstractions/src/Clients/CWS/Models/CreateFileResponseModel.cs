using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class CreateFileResponseModel : IMasterDataModel
  {
    /// <summary>
    /// FilespaceId in DataOcean
    /// </summary>
    [JsonProperty("filespaceId")]
    public string FileSpaceId { get; set; }

    /// <summary>
    /// FilespaceId within dataocean
    /// </summary>
    [JsonProperty("uploadUrl")]
    public string UploadUrl { get; set; }

    public List<string> GetIdentifiers() => new List<string> { FileSpaceId };
  }

  /* example
    {
     "filespaceId": "8e2da-db88-497b-8d3c-7bfd87",
     "uploadUrl": "fileUrL"
    }
   */
}
