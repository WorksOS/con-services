using System;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexSubGridTreeException : TRexException
  {
    public TRexSubGridTreeException(string message) : base(message)
    {
    }

    public TRexSubGridTreeException(string message, Exception E) : base(message, E)
    {
    }
  }
}
