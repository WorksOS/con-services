using System;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexFilterPreparationException : TRexException
  {
    private const string ERROR_MESSAGE = "An exception occured during filter preparation phase";

    public TRexFilterPreparationException(string message = ERROR_MESSAGE) : base(message)
    {
    }

    public TRexFilterPreparationException(string message, Exception E) : base(message, E)
    {
    }
  }
}
