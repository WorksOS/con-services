using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectConfigurationFileListResponseModel : IMasterDataModel
  {
    public ProjectConfigurationFileListResponseModel()
    {
      ProjectConfigurationFiles = new List<ProjectConfigurationFileResponseModel>();
    }

    /// <summary>
    /// projectConfigurationFiles
    /// </summary>
    [JsonProperty("projectConfigurationFiles")]
    public List<ProjectConfigurationFileResponseModel> ProjectConfigurationFiles { get; set; }


    public List<string> GetIdentifiers() => ProjectConfigurationFiles?
                                           .SelectMany(a => a.GetIdentifiers())
                                           .Distinct()
                                           .ToList()
                                         ?? new List<string>();
  }

  /* example

   [
    {
        "fileType": "CALIBRATION",
        "fileName": "trn::profilex:us-west-2:project:2092b1a9-e4d6-41e5-b210-b8fff3e922da||2020-04-20 23:30:28.253||BootCamp 2012.dc",
        "fileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/a91de9b5-e7b7-432e-ae99-74f6946d6d21?Signature=RaSFg-l65~7oHmlxissJZeDjqUrz5PHIX-~jN5LPMWIX1lUtYcKVcsOjn1x2UqNwsRjPcs7NR4XCsI5yI9Rkj0Wkt80wrf0HqMfPwRqUZQf9jZ~YfBK-gbSin5yZ-wXI6gOpSWrG~16CHLCnOp204-eJml3esrzw8RavQJoKHu7YQiyDUvRhFoNeD5FuTSgOXln0TVRXxVrVsT1lmCsZ8jiK39FvEZ8BWA4o1VHYlQQY1xs9gmB9dVK0Ns4hAAX12F1fz1eKt5UQnvuFXuQxS2axMrthKnaNPGj-sunvCjBLD0bDBEeNJfEZL-1FOPkSlsmC76pLU-1coXfhK8EUBg__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYTkxZGU5YjUtZTdiNy00MzJlLWFlOTktNzRmNjk0NmQ2ZDIxIiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg3NTExODI4fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
        "md5": "7f5dcb4273fded769f05dbd30caa3423",
        "size": 888,
        "siteCollectorFileName": "trn::profilex:us-west-2:project:2092b1a9-e4d6-41e5-b210-b8fff3e922da||2020-04-20 23:30:28.253||BootCamp 2012.dc",
        "siteCollectorFileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/a91de9b5-e7b7-432e-ae99-74f6946d6d21?Signature=RaSFg-l65~7oHmlxissJZeDjqUrz5PHIX-~jN5LPMWIX1lUtYcKVcsOjn1x2UqNwsRjPcs7NR4XCsI5yI9Rkj0Wkt80wrf0HqMfPwRqUZQf9jZ~YfBK-gbSin5yZ-wXI6gOpSWrG~16CHLCnOp204-eJml3esrzw8RavQJoKHu7YQiyDUvRhFoNeD5FuTSgOXln0TVRXxVrVsT1lmCsZ8jiK39FvEZ8BWA4o1VHYlQQY1xs9gmB9dVK0Ns4hAAX12F1fz1eKt5UQnvuFXuQxS2axMrthKnaNPGj-sunvCjBLD0bDBEeNJfEZL-1FOPkSlsmC76pLU-1coXfhK8EUBg__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYTkxZGU5YjUtZTdiNy00MzJlLWFlOTktNzRmNjk0NmQ2ZDIxIiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg3NTExODI4fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
        "siteCollectorMd5": "7f5dcb4273fded769f05dbd30caa3423",
        "siteCollectorSize": 888,
        "createdAt": "2020-04-20T23:30:28.421Z",
        "updatedAt": "2020-04-20T23:30:32.814Z"
    },
    {
        "fileType": "AVOIDANCE_ZONE",
        "fileName": null,
        "fileDownloadLink": null,
        "md5": null,
        "size": 0,
        "siteCollectorFileName": "trn::profilex:us-west-2:project:2092b1a9-e4d6-41e5-b210-b8fff3e922da||2020-04-20 23:33:10.917||Dimensions_2012_LineString.avoid.dxf",
        "siteCollectorFileDownloadLink": "https://fs-ro-us1.staging-tdata-cdn.com/r/4604d944-d7e5-4b0d-a89c-8bd037c2e25c?Signature=phvQ4hZmE4cT~KexPDC7a2YqZR7cjRBOILSyLGSn2lTwUq5dappU2nUAqEGuK8m0ua~URJv1U4RF4PhCUhcwdIQcAR4ougSYOjrunaTUXX6JiAHgZQr4Grn~-fkF~m05-fcnzbL9Hg-AmGkvHIosKIGvf0yxuu~jb9tt9542UhyMaQM7WEGHh8Heat2ccCfPCZEKttV6SjlKjikk85azKuaH9wYfj9Cq4NuznlxPagewQET9Uu27EZi6q6~PWkiRCmlWiQ079B-7A5F0VydnMmj7wagsOwuuoylMJM3hyswYx11rwMDmAWJPAL1cSxsHp6m9perP8MttotJEAvdq5A__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvNDYwNGQ5NDQtZDdlNS00YjBkLWE4OWMtOGJkMDM3YzJlMjVjIiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg3NTExOTkxfX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ",
        "siteCollectorMd5": "5c8067331949f79a5cecf0df2a12fa1d",
        "siteCollectorSize": 40596,
        "createdAt": "2020-04-20T23:33:11.102Z",
        "updatedAt": "2020-04-20T23:33:15.225Z"
    }
  ]

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
