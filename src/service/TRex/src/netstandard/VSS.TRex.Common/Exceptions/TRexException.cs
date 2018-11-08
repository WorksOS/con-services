using System;

namespace VSS.TRex.Exceptions
{
  /// <summary>
  /// The base class for TRex custom exceptions. Rarrr!
  /// </summary>
  public class TRexException : Exception
  {
    public TRexException(string message) : base(message)
    {
    }

    public TRexException(string message, Exception E) : base(message, E)
    {
    }
  }
}
