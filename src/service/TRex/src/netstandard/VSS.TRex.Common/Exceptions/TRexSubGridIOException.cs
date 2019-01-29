using System;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexSubGridIOException : Exception
  {
    public TRexSubGridIOException(string message) : base(message)
    {
    }

    public TRexSubGridIOException(string message, Exception E) : base(message, E)
    {
    }
  }
}
