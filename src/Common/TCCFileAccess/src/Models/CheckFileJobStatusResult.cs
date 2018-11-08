using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class CheckFileJobStatusResult : ApiResult
  {
    public DateTime? createTime;
    public DateTime? startTime;
    public DateTime? endTime;
    public List<string> failedTiles;
    public int filestoproceed;
    public string memberdisplayname;
    public string memberId;
    public int pendingtiles;
    public int progress;
    public string status;
    public List<RenderOutputInfo> renderOutputInfo;
  }

  public class RenderOutputInfo
  {
    public string fileId;
    [JsonProperty(PropertyName = "params")]
    public object Params;

  }
}
