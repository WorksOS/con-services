using System;

namespace VSS.TRex.Common.Exceptions.Exceptions
{
  public class TRexClientLeafSubGridTypeCastException : TRexClientLeafSubGridException
  {
    private const string ERROR_MESSAGE_TEMPLATE = "Invalid ClientLeafSubGrid type: {0}. Expected type: {1}";

    public TRexClientLeafSubGridTypeCastException(string actualType, string expectedType, Exception e) 
      : base(string.Format(ERROR_MESSAGE_TEMPLATE, actualType, expectedType), e)
    {
    }

    public TRexClientLeafSubGridTypeCastException(string actualType, string expectedType) 
      : base(string.Format(ERROR_MESSAGE_TEMPLATE, actualType, expectedType))
    {
    }
  }
}
