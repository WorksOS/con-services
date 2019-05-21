using System;

namespace VSS.TRex.Common.Exceptions.Exceptions
{
  public class TRexClientLeafSubGridTypeCastException : TRexClientLeafSubGridException
  {
    public TRexClientLeafSubGridTypeCastException(string actualType, string expectedType) 
      : base($"Invalid ClientLeafSubGrid type: {actualType}. Expected type: {expectedType}")
    {
    }
  }
}
