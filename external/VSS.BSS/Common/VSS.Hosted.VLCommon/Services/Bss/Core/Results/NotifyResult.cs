using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class NotifyResult : ActivityResult
  {
    public NotifyResult()
    {
      Type = ResultType.Notify;
    }

    public Exception Exception { get; set; }
    public override string Summary
    {
      get
      {
        if (Exception != null)
          return string.IsNullOrWhiteSpace(base.Summary)
            ? Exception.Message
            : string.Join(" ", base.Summary, Exception.Message);
        return base.Summary;
      }
    }
  }
}