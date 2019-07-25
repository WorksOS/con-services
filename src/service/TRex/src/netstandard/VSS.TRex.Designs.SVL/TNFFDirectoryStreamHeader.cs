using System.Runtime.InteropServices;

namespace VSS.TRex.Designs.SVL
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TNFFDirectoryStreamHeader
  {
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = NFFConsts.kTNFFMagicNumberArraySize)]
  public byte[] MajicNumber;  // TNFFMagicNumberType;
  }
}
