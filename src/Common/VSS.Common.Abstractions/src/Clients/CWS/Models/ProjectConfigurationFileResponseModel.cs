using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectConfigurationFileResponseModel : IMasterDataModel
  {
    /// <summary>
    /// fileName
    /// </summary>
    [JsonProperty("fileName")]
    public string FileName { get; set; }

    /// <summary>
    /// url from dataocean
    /// </summary>
    [JsonProperty("fileDownloadLink")]
    public string FileDownloadLink { get; set; }

    /// <summary>
    /// fileType ProjectCalibrationFileType
    /// </summary>
    [JsonProperty("fileType")]
    public string FileType { get; set; }

    /// <summary>
    /// needed?
    /// </summary>
    [JsonProperty("siteCollectorFileName")]
    public string SiteCollectorFileName { get; set; }

    /// <summary>
    /// needed?
    /// </summary>
    [JsonProperty("siteCollectorFileDownloadLink")]
    public string SiteCollectorFileDownloadLink { get; set; }


    /// <summary>
    /// todoJeannie what is this?
    /// </summary>
    [JsonProperty("md5")]
    public string Md5 { get; set; }

    /// <summary>
    /// create UTC?
    /// </summary>
    [JsonProperty("createdAt")]
    public string CreatedAt { get; set; }

    /// <summary>
    /// udpate UTC?
    /// </summary>
    [JsonProperty("updatedAt")]
    public string UpdatedAt { get; set; }

    /// <summary>
    /// file size
    /// </summary>
    [JsonProperty("size")]
    public string Size { get; set; }


    public List<string> GetIdentifiers() => new List<string> { FileName }; // todo something unique?
  }

  /* example
  {
    "fileName": "string",
    "fileDownloadLink": "string",
    "fileType": "CALIBRATION",
    "siteCollectorFileName": "string",
    "siteCollectorFileDownloadLink": "string",
    "md5": "string",
    "createdAt": "string",
    "updatedAt": "string",
    "size": 0
  }
  */
}
