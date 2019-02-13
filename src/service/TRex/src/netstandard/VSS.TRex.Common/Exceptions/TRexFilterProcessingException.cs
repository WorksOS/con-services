using System;
using VSS.TRex.Exceptions;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexFilterProcessingException : TRexException
  {
    private const string ERROR_MESSAGE = "An exception occured during filter processing";

    public TRexFilterProcessingException(string message = ERROR_MESSAGE) : base(message)
    {
    }

    public TRexFilterProcessingException(string message, Exception E) : base(message, E)
    {
    }
  }
}
