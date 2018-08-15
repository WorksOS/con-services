using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
  public static class Utilities
  {
    /// <summary>
    /// Reads a single float number from the stream taking into account the the size (single or double) of the number in the stream
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="ValueSize"></param>
    /// <returns></returns>
    public static double ReadFloat(BinaryReader reader, short ValueSize) => ValueSize == sizeof(float) ? reader.ReadSingle() : reader.ReadDouble();

    /// <summary>
    /// Reads a single integer float number from the stream taking into account the the size (single or double) of the number in the stream
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="ValueSize"></param>
    /// <returns></returns>
    public static int ReadInteger(BinaryReader reader, short ValueSize) => ValueSize == sizeof(short) ? reader.ReadInt16() : reader.ReadInt32();
  }
}
