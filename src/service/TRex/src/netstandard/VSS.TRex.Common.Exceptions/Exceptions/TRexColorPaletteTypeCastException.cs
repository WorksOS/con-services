using System;

namespace VSS.TRex.Common.Exceptions.Exceptions
{
  public class TRexColorPaletteTypeCastException : TRexColorPaletteException
  {
    public TRexColorPaletteTypeCastException(string actualType, string expectedType) 
      : base($"Invalid Palette type: {actualType}. Expected type: {expectedType}")
    {
    }
  }
}
