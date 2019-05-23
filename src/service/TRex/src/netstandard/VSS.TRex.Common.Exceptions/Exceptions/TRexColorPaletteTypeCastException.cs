using System;

namespace VSS.TRex.Common.Exceptions.Exceptions
{
  public class TRexColorPaletteTypeCastException : TRexColorPaletteException
  {
    private const string ERROR_MESSAGE_TEMPLATE = "Invalid Palette type: {0}. Expected type: {1}";

    public TRexColorPaletteTypeCastException(string actualType, string expectedType, Exception e)
    : base(string.Format(ERROR_MESSAGE_TEMPLATE, actualType, expectedType), e)
    {
    }

    public TRexColorPaletteTypeCastException(string actualType, string expectedType) 
      : base(string.Format(ERROR_MESSAGE_TEMPLATE, actualType, expectedType))
    {
    }
  }
}
