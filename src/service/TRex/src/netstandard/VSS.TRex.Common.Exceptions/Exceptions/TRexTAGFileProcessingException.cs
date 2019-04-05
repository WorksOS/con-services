using System;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexTAGFileProcessingException : TRexException
  {
    public TRexTAGFileProcessingException(string message) : base(message)
    {
    }

    public TRexTAGFileProcessingException(string message, Exception E) : base(message, E)
    {
    }
  }
}
