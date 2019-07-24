using System.Runtime.InteropServices;

namespace VSS.TRex.Designs.SVL
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TNFFLineworkGridFileHeader
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = NFFConsts.kTNFFMagicNumberArraySize)]
    public byte[] MajicNumber; // TNFFMagicNumberType

    public byte MajorVer;
    public byte MinorVer;
    public TNFFAbsolute2dIntegerCoordinate Origin;
  }
}
