using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Velociraptor.Designs.TTM
{
    public static class Utilities
    {
        public static void WriteFloat(BinaryWriter writer, double Value, short ValueSize)
        {
            if (ValueSize == sizeof(float))
            {
                float aSingle = (float)Value;
                writer.Write(aSingle);
            }
            else
            {
                writer.Write(Value);
            }
        }

        public static double ReadFloat(BinaryReader reader, short ValueSize) 
        {
            return ValueSize == sizeof(float) ? reader.ReadSingle() : reader.ReadDouble();
        }

        public static void WriteInteger(BinaryWriter writer, int Value, short ValueSize)
        {
            if (ValueSize == sizeof(short))
            {
                short aShort = (short)Value;
                writer.Write(aShort);
            }
            else
            {
                writer.Write(Value);
            }
        }

        public static int ReadInteger(BinaryReader reader, short ValueSize)
        {
            return ValueSize == sizeof(short) ? reader.ReadInt16() : reader.ReadInt32();
        }
    }
}
