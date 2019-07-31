using System.Runtime.InteropServices;

namespace VSS.TRex.Designs.SVL
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct NFFDirectoryStreamHeader
  {
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = NFFConsts.kNFFMagicNumberArraySize)]
  public byte[] MagicNumber;  
  }
}
