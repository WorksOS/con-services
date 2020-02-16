using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ExceptionResult : ActivityResult
  {
    public ExceptionResult()
    {
      Type = ResultType.Exception;
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