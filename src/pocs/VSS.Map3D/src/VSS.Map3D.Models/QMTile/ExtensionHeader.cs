using System.IO;
using System.Runtime.InteropServices;

namespace VSS.Map3D.Models.QMTile
{

  /*
  Extension data may follow to supplement the quantized-mesh with additional information.
  Each extension begins with an ExtensionHeader, consisting of a unique identifier and the size of the extension data in bytes. An unsigned char is a 8-bit unsigned integer.
  */

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct ExtensionHeader
  {
    public byte extensionId;
    public uint extensionLength;

    public ExtensionHeader(BinaryReader reader)
    {
      extensionId = reader.ReadByte();
      extensionLength = reader.ReadUInt32();
    }
  }
}
