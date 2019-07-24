using System.Runtime.InteropServices;

namespace VSS.TRex.Designs.SVL
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TNFFAbsolute2dIntegerCoordinate
  {
    public int X;
    public int Y;
  }
}