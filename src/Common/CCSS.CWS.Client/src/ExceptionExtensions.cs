using System;

namespace CCSS.CWS.Client
{
  public static class ExceptionExtensions
  {
    public static bool IsNotFoundException(this Exception e)
    {
      return e.Message.Contains("404") || e.Message.Contains("NotFound");
    }
  }
}
