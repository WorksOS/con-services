using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectConfigurationListResponseModel : IMasterDataModel
  {

    public ProjectConfigurationListResponseModel()
    {
      ProjectConfigurations = new List<ProjectConfiguration>();
    }

    /// <summary>
    /// Project configuration file list
    /// </summary>
    [JsonProperty("config")]
    public List<ProjectConfiguration> ProjectConfigurations { get; set; }

    /// <summary>
    /// Returned as true if the result has more records to display. Helps in pagination. False implies that there are no more records to display.
    /// </summary>
    [JsonProperty("hasMore")]
    public bool HasMore { get; set; }

    public List<string> GetIdentifiers() => ProjectConfigurations?
                                              .SelectMany(d => d.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }

  public class ProjectConfiguration : IMasterDataModel
  {
    //Note: There are other properties returned but we only want some of it

    public static string FilenamePathSeparator = "||";

    /// <summary>
    /// File type. e.g. CALIBRATION?
    /// </summary>
    [JsonProperty("fileType")]
    public string FileType { get; set; }

    /// <summary>
    /// File name made up of projectTrn, timestamp and filename
    ///    I believe FileName/FileDownloadLink/Size/FileDownloadLink
    ///          are used for if there is 1 and only 1 for a type,
    ///          or if there is also a siteCollector file, this will be the machineControl file
    /// </summary>
    [JsonProperty("fileName")]
    public string FileName { get; set; }

    /// <summary>
    /// S3 url
    /// </summary>
    [JsonProperty("fileDownloadLink")]
    public string FileDownloadLink { get; set; }

    /// <summary>
    /// Size
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

    // this should be unique due to it including date
    public List<string> GetIdentifiers() => new List<string> { FileName };

  }
}

/* example
{
    "projectId": "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f",
    "accountId": "trn::profilex:us-west-2:account:158ef953-4967-4af7-81cc-952d47cb6c6f",
    "accountName": "WM TEST TRIMBLECEC MAR 26",
    "projectName": "JeannieTest",
    "lastUpdate": "2020-03-25T23:03:42Z",
    "projectStartDate": null,
    "projectEndDate": null,
    "projectUnits": null,
    "userCount": null,
    "deviceCount": null,
    "profileImage": null,
    "role": null,
    "projectSettings": {
        "projectId": "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f",
        "unit": null,
        "timezone": "Pacific/Auckland",
        "boundary": {
            "type": "Polygon",
            "coordinates": [
                [
                    [
                        172.506537618602,
                        -43.574201413895835
                    ],
                    [
                        172.5065180906021,
                        -43.574220941895746
                    ],
                    [
                        172.51470032256623,
                        -43.57834134987768
                    ],
                    [
                        172.51579389056144,
                        -43.577247781882484
                    ],
                    [
                        172.5171022665557,
                        -43.57326406989995
                    ],
                    [
                        172.51020888258589,
                        -43.570120061913734
                    ],
                    [
                        172.506537618602,
                        -43.574201413895835
                    ]
                ]
            ]
        },
        "boundaryCentroid": {
            "longitude": 172.51223648244397,
            "latitude": -43.574147402337694
        },
        "boundaryCentroidTimezone": "Pacific/Auckland",
        "boundaryArea": 422666.6688001421,
        "config": [
            {
                "fileType": "CALIBRATION",
                "fileName": "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
                "fileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/af390a82-8cc2-4486-aba8-e66a2dcfa3f8?Signature=ba2WBHiNp2FwMUGiKqPl6B6hbLikXRvL9MJde0OMmyKzpEiPKj01TmOaaTqC9B~xTsnr5g6GIcWSa7I1bd5sUO6lqWPVTA~rDC-MBqh6BVVLzC6ed2Ny5slUkePCj3cA1QbQiwVsAXIgO1eRQK-xcqJf1JLEc9C5G7c164uZGmmrJ2C1d4yftBau8-Fd0YItOH33l8bpvv~SE1nnvJ-iu4Hc8XqokJNTjdqY3TTQD45zTiJ1icYxRgfyjJIrgVi0IZH247qLtm8R-VtAPKS0HetfXhFhThUUFjssJ-Mxha5RBnZu4ATAr8SX-eLfqXXVBzWdJ4PyLg6-dKrCCfdo~Q__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYWYzOTBhODItOGNjMi00NDg2LWFiYTgtZTY2YTJkY2ZhM2Y4IiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg5NTA3NDc0fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
                "md5": "7f5dcb4273fded769f05dbd30caa3423",
                "size": 888,
                "siteCollectorFileName": "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
                "siteCollectorFileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/af390a82-8cc2-4486-aba8-e66a2dcfa3f8?Signature=ba2WBHiNp2FwMUGiKqPl6B6hbLikXRvL9MJde0OMmyKzpEiPKj01TmOaaTqC9B~xTsnr5g6GIcWSa7I1bd5sUO6lqWPVTA~rDC-MBqh6BVVLzC6ed2Ny5slUkePCj3cA1QbQiwVsAXIgO1eRQK-xcqJf1JLEc9C5G7c164uZGmmrJ2C1d4yftBau8-Fd0YItOH33l8bpvv~SE1nnvJ-iu4Hc8XqokJNTjdqY3TTQD45zTiJ1icYxRgfyjJIrgVi0IZH247qLtm8R-VtAPKS0HetfXhFhThUUFjssJ-Mxha5RBnZu4ATAr8SX-eLfqXXVBzWdJ4PyLg6-dKrCCfdo~Q__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYWYzOTBhODItOGNjMi00NDg2LWFiYTgtZTY2YTJkY2ZhM2Y4IiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg5NTA3NDc0fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
                "siteCollectorMd5": "7f5dcb4273fded769f05dbd30caa3423",
                "siteCollectorSize": 888
            }
        ]
    },
    "createdAt": null,
    "createdBy": null
}
*/
