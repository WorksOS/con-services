using System;

namespace VSS.TRex.Common.Exceptions
{
  /// <summary>
  /// The base class for TRex TIN exceptions.
  /// </summary>
  public class TRexTINException : Exception
  {
    public TRexTINException(string message) : base(message)
    {
    }

    public TRexTINException(string message, Exception E) : base(message, E)
    {
    }
  }
}
