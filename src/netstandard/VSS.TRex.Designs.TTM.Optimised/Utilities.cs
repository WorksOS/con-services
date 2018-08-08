using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
    public static class Utilities
    {
        public static double ReadFloat(BinaryReader reader, short ValueSize) 
        {
            return ValueSize == sizeof(float) ? reader.ReadSingle() : reader.ReadDouble();
        }

        public static int ReadInteger(BinaryReader reader, short ValueSize)
        {
            return ValueSize == sizeof(short) ? reader.ReadInt16() : reader.ReadInt32();
        }
    }
}
