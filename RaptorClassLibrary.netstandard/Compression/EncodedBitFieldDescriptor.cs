using System;
using System.IO;

namespace VSS.TRex.Compression
{
    public struct EncodedBitFieldDescriptor
    {
        public int NativeNullValue;
        public int EncodedNullValue;
        public int MinValue, MaxValue;

        public bool Nullable;
        public bool AllValuesAreNull;

        // OffsetBits notes the number of bits from the start of the record this field begins
        public ushort OffsetBits;
        public byte RequiredBits;

        public int NumValues => MaxValue - MinValue + 1;

        public void Init()
        {
            Nullable = false;
            NativeNullValue = 0;
            EncodedNullValue = 0;
            MinValue = 0;
            MaxValue = 0;
            OffsetBits = 0;
            AllValuesAreNull = false;
            RequiredBits = 0;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((long)NativeNullValue);
            writer.Write((long)EncodedNullValue);
            writer.Write((long)MinValue);
            writer.Write((long)MaxValue);

            writer.Write(Nullable);
            writer.Write(AllValuesAreNull);

            writer.Write(OffsetBits);
            writer.Write(RequiredBits);
        }

        public void Read(BinaryReader reader)
        {
            NativeNullValue = (int)reader.ReadInt64();
            EncodedNullValue = (int)reader.ReadInt64();
            MinValue = (int)reader.ReadInt64();
            MaxValue = (int)reader.ReadInt64();

            Nullable = reader.ReadBoolean();
            AllValuesAreNull = reader.ReadBoolean();

            OffsetBits = reader.ReadUInt16();
            RequiredBits = reader.ReadByte();
        }

        public void CalculateRequiredBitFieldSize()
        {
            AllValuesAreNull = Nullable && ((MinValue == NativeNullValue) || (MaxValue == NativeNullValue));

            if (AllValuesAreNull || MinValue == MaxValue) // no storage required, 0 bits to encode this attribute
            {
                RequiredBits = 0;
                return;
            }

            int ValueRange = MaxValue - MinValue; // Represented by 0-based indices so no need to add 1. See Bug 24559.

            // Calculate the ceiling of the base 2 log of the numeric range from MinValue to MaxValue
            RequiredBits = (byte)(1 + (int)Math.Floor(Math.Log(ValueRange, 2)));
        }
    }
}
