using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectConfigurationModel : IMasterDataModel
  {
    //Note: There are other properties returned but we only want some of it

    public static string FilenamePathSeparator = "||";

    /// <summary>
    /// File name made up of projectTrn, timestamp and filename
    ///    I believe FileName/FileDownloadLink/Size/FileDownloadLink
    ///          are used for if there is 1 and only 1 for a type,
    ///          or if there is also a siteCollector file, this will be the machineControl file
    /// </summary>
    [JsonProperty("fileName")]
    public string FileName { get; set; }


    /// <summary>
    /// File type. e.g. CALIBRATION?
    /// </summary>
    [JsonProperty("fileType")]
    public string FileType { get; set; }

    /// <summary>
    /// S3 url
    /// </summary>
    [JsonProperty("fileDownloadLink")]
    public string FileDownloadLink { get; set; }

    /// <summary>
    /// File Size to compare with downloaded file
    /// </summary>
    [JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// checksum to compare with downloaded file
    /// </summary>
    [JsonProperty("md5")]
    public string Md5 { get; set; }

    /// <summary>
    /// File name made up of projectTrn, timestamp and filename
    ///    Refers to siteCollector file
    /// </summary>
    [JsonProperty("siteCollectorFileName")]
    public string SiteCollectorFileName { get; set; }

    /// <summary>
    /// S3 url
    /// </summary>
    [JsonProperty("siteCollectorFileDownloadLink")]
    public string SiteCollectorFileDownloadLink { get; set; }

    /// <summary>
    /// Size of SC file
    /// </summary>
    [JsonProperty("siteCollectorSize")]
    public int SiteCollectorSize { get; set; }

    /// <summary>
    /// S3 url
    /// </summary>
    [JsonProperty("siteCollectorMd5")]
    public string SiteCollectorMd5 { get; set; }

    /// <summary>
    /// create UTC?
    /// </summary>
    [JsonProperty("createdAt")]
    public string CreatedAt { get; set; }

    /// <summary>
    /// update UTC?
    /// </summary>
    [JsonProperty("updatedAt")]
    public string UpdatedAt { get; set; }

    // this should be unique due to it including date
    public List<string> GetIdentifiers() => new List<string> { FileName };

  }
}

/* example
{
  "fileType": "CALIBRATION",
  "fileName": "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
  "fileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/af390a82-8cc2-4486-aba8-e66a2dcfa3f8?Signature=ba2WBHiNp2FwMUGiKqPl6B6hbLikXRvL9MJde0OMmyKzpEiPKj01TmOaaTqC9B~xTsnr5g6GIcWSa7I1bd5sUO6lqWPVTA~rDC-MBqh6BVVLzC6ed2Ny5slUkePCj3cA1QbQiwVsAXIgO1eRQK-xcqJf1JLEc9C5G7c164uZGmmrJ2C1d4yftBau8-Fd0YItOH33l8bpvv~SE1nnvJ-iu4Hc8XqokJNTjdqY3TTQD45zTiJ1icYxRgfyjJIrgVi0IZH247qLtm8R-VtAPKS0HetfXhFhThUUFjssJ-Mxha5RBnZu4ATAr8SX-eLfqXXVBzWdJ4PyLg6-dKrCCfdo~Q__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYWYzOTBhODItOGNjMi00NDg2LWFiYTgtZTY2YTJkY2ZhM2Y4IiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg5NTA3NDc0fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
  "md5": "7f5dcb4273fded769f05dbd30caa3423",
  "size": 888,
  "siteCollectorFileName": "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
  "siteCollectorFileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/af390a82-8cc2-4486-aba8-e66a2dcfa3f8?Signature=ba2WBHiNp2FwMUGiKqPl6B6hbLikXRvL9MJde0OMmyKzpEiPKj01TmOaaTqC9B~xTsnr5g6GIcWSa7I1bd5sUO6lqWPVTA~rDC-MBqh6BVVLzC6ed2Ny5slUkePCj3cA1QbQiwVsAXIgO1eRQK-xcqJf1JLEc9C5G7c164uZGmmrJ2C1d4yftBau8-Fd0YItOH33l8bpvv~SE1nnvJ-iu4Hc8XqokJNTjdqY3TTQD45zTiJ1icYxRgfyjJIrgVi0IZH247qLtm8R-VtAPKS0HetfXhFhThUUFjssJ-Mxha5RBnZu4ATAr8SX-eLfqXXVBzWdJ4PyLg6-dKrCCfdo~Q__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYWYzOTBhODItOGNjMi00NDg2LWFiYTgtZTY2YTJkY2ZhM2Y4IiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg5NTA3NDc0fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
  "siteCollectorMd5": "7f5dcb4273fded769f05dbd30caa3423",
  "siteCollectorSize": 888,
  "createdAt": "2020-03-25T23:03:45.574Z",
  "updatedAt": "2020-03-25T23:03:50.033Z"
}
*/
