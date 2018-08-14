using System;
using VSS.TRex.Exceptions;

namespace VSS.TRex.Designs.TTM.Optimised.Exceptions
{
  /// <summary>
  /// Generic TTM read exception thrown while reading in a TTM fiile
  /// </summary>
    public class TTMFileReadException : TRexException
    {
      public TTMFileReadException(string message) : base(message)
      {
      }

      public TTMFileReadException(string message, Exception E) : base(message, E)
      {
      }
  }
}
