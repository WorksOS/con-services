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

    public List<string> GetIdentifiers() => new List<string> { FileName };
  }

  /* example
  {
    "fileType": "CALIBRATION",
    "fileName": "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
    "fileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/af390a82-8cc2-4486-aba8-e66a2dcfa3f8?Signature=eVLwMzTwyAlUg~ClgMu2V1BD0QqtwiNDHD~323QfKZw5bEYHs329k2E2fwbarld3HhhoV9xuBFuom6YHGfd7Tlj4j9nFC~8vl4bh0oFsuZF0DsVG0PBKWeQmOnWGvw-HbyRYqstJa5QybeGT1B8JnJG9ApMmBUkC0Myb2nTTbirCgz1mHZ2~kSPe8gqY5WNH~1pRXhB7NeEdYr76~rVr5zlwMcesKoSxPhKVuwBDy5P7rtY-NfbHg5-bSB703bvDCdANrZAw4zTItg0Z9fsa~YiSdKyaaaetPc9PkY7Wkbo048VWXiyM3yRAM0jamN4txTTQjPs3WcpTBqRWxz-mEw__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYWYzOTBhODItOGNjMi00NDg2LWFiYTgtZTY2YTJkY2ZhM2Y4IiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg3NTA3NzY1fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
    "md5": "7f5dcb4273fded769f05dbd30caa3423",
    "size": 888,
    "siteCollectorFileName": "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
    "siteCollectorFileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/af390a82-8cc2-4486-aba8-e66a2dcfa3f8?Signature=eVLwMzTwyAlUg~ClgMu2V1BD0QqtwiNDHD~323QfKZw5bEYHs329k2E2fwbarld3HhhoV9xuBFuom6YHGfd7Tlj4j9nFC~8vl4bh0oFsuZF0DsVG0PBKWeQmOnWGvw-HbyRYqstJa5QybeGT1B8JnJG9ApMmBUkC0Myb2nTTbirCgz1mHZ2~kSPe8gqY5WNH~1pRXhB7NeEdYr76~rVr5zlwMcesKoSxPhKVuwBDy5P7rtY-NfbHg5-bSB703bvDCdANrZAw4zTItg0Z9fsa~YiSdKyaaaetPc9PkY7Wkbo048VWXiyM3yRAM0jamN4txTTQjPs3WcpTBqRWxz-mEw__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYWYzOTBhODItOGNjMi00NDg2LWFiYTgtZTY2YTJkY2ZhM2Y4IiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg3NTA3NzY1fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
    "siteCollectorMd5": "7f5dcb4273fded769f05dbd30caa3423",
    "siteCollectorSize": 888,
    "createdAt": "2020-03-25T23:03:45.574Z",
    "updatedAt": "2020-03-25T23:03:50.033Z"
  }

  or
  {
    "status": 404,
    "code": 11012,
    "moreInfo": "Please provide this id to support, while contacting, TraceId 5e9e3000802cef392f29840ca0864165",
    "message": "Config File not found for given project Id",
    "timestamp": "2020-04-20T23:28:00.050+0000"
   }
    

  */
}
