using System;

namespace VSS.TRex.Common.Exceptions.Exceptions
{
  public class TRexColorPaletteException : TRexException
  {
    public TRexColorPaletteException(string message) : base(message)
    {
    }

    public TRexColorPaletteException(string message, Exception e) : base(message, e)
    {
    }
  }
}
