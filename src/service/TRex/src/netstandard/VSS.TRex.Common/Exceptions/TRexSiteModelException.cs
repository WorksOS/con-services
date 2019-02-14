using System;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexSiteModelException : Exception
  {
    public TRexSiteModelException(string message) : base(message)
    {
    }

    public TRexSiteModelException(string message, Exception E) : base(message, E)
    {
    }
  }
}
