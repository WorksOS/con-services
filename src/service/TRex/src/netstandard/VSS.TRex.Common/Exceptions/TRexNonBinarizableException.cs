using System;
using VSS.TRex.Exceptions;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexNonBinarizableException : TRexException
  {
    private const string ERROR_MESSAGE = "The content is no binarizably serializable";

    public TRexNonBinarizableException(string message = ERROR_MESSAGE) : base(message)
    {
    }

    public TRexNonBinarizableException(string message, Exception E) : base(message, E)
    {
    }
  }
}
