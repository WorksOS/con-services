using System;

namespace VSS.TRex.Common.Exceptions.Exceptions
{
  public class TRexClientLeafSubGridException : TRexException
  {
    public TRexClientLeafSubGridException(string message) : base(message)
    {
    }

    public TRexClientLeafSubGridException(string message, Exception E) : base(message, E)
    {
    }
  }
}
