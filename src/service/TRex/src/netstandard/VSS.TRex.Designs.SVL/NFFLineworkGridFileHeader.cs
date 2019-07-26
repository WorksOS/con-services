using System.Runtime.InteropServices;

namespace VSS.TRex.Designs.SVL
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct NFFLineworkGridFileHeader
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = NFFConsts.kNFFMagicNumberArraySize)]
    public byte[] MajicNumber;

    public byte MajorVer;
    public byte MinorVer;
    public NFFAbsolute2dIntegerCoordinate Origin;
  }
}
