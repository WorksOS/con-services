using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ActivityResult
  {
    public DateTime DateTimeUtc { get; protected set; }
    public ResultType Type { get; protected set; }
    public virtual string Summary { get; set; }

    public ActivityResult()
    {
      DateTimeUtc = DateTime.UtcNow;
      Type = ResultType.Information;
    }
  }
}