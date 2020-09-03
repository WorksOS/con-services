using System;

namespace VSS.TRex.Common.Exceptions.Exceptions
{
  public class TRexRequestLivelinessException : TRexException
  {
    public TRexRequestLivelinessException(string message) : base(message)
    {
    }

    public TRexRequestLivelinessException(string message, Exception e) : base(message, e)
    {
    }
  }
}
