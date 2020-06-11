using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class GetFileResponseModel : IMasterDataModel
  {
    /// <summary>
    /// FilespaceId in DataOcean
    /// </summary>
    [JsonProperty("filespaceId")]
    public string FileSpaceId { get; set; }

    /// <summary>
    /// URL to use for downloading file
    /// </summary>
    [JsonProperty("downloadUrl")]
    public string DownloadUrl { get; set; }

    /// <summary>
    /// CWS File name of format
    /// e.g. trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc
    /// </summary>
    [JsonProperty("fileName")]
    public string FileName { get; set; }

    /// <summary>
    /// this can be used for checksum validation
    /// </summary>
    [JsonProperty("md5")]
    public string Md5 { get; set; }

    public List<string> GetIdentifiers() => new List<string> { FileSpaceId };
  }

  /* example
    {
      "filespaceId": "string",
      "fileName": "string", //TODO: waiting on CWS changes
      "downloadUrl": "string",
      "md5": "string"
    }
   */
}
