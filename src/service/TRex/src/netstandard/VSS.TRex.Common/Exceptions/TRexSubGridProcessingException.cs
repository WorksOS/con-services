using System;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexSubGridProcessingException : TRexException
  {
    public TRexSubGridProcessingException(string message) : base(message)
    {
    }

    public TRexSubGridProcessingException(string message, Exception E) : base(message, E)
    {
    }
  }
}
