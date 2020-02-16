using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public sealed class BitShifter
  {
    public static void SetValue(SByte number, int bitIndex, int bitLength, byte[] buffer)
    {
      SetValue((UInt32)number, bitIndex, bitLength, buffer);
    }

    public static void SetValue(Byte number, int bitIndex, int bitLength, byte[] buffer)
    {
      SetValue((UInt32)number, bitIndex, bitLength, buffer);
    }

    public static void SetValue(Int16 number, int bitIndex, int bitLength, byte[] buffer)
    {
      SetValue((UInt32)number, bitIndex, bitLength, buffer);
    }

    public static void SetValue(UInt16 number, int bitIndex, int bitLength, byte[] buffer)
    {
      SetValue((UInt32)number, bitIndex, bitLength, buffer);
    }

    public static void SetValue(Int32 number, int bitIndex, int bitLength, byte[] buffer)
    {
      SetValue((UInt32)number, bitIndex, bitLength, buffer);
    }

    public static void SetValue(bool boolean, int bitIndex, int bitLength, byte[] buffer)
    {
      if (boolean == true)
      {
        SetValue(1, bitIndex, bitLength, buffer);
      }
      else
      {
        SetValue(0, bitIndex, bitLength, buffer);
      }
    }

    /// <summary>
    /// Overwrites buffer with "number" at the specified "bitIndex", consuming
    /// the specified "bitLength". Buffer is regarded as a array of bits. The number
    /// may be written across byte boundaries.
    /// </summary>
    public static void SetValue(UInt32 number, int bitIndex, int bitLength, byte[] buffer)
    {
      if (((buffer.Length * bitsPerByte) < (bitIndex + bitLength)) ||
        (bitLength > bitsPerInt))
      {
        throw new ArgumentOutOfRangeException();
      }

      if (BitConverter.IsLittleEndian == false)
      {
        throw new ArithmeticException("Non-portable function being called on non-Intel CPU");
      }

      // Locate the byte in the buffer to set, and the bit within that byte
      int byteIndex = (int)(bitIndex / 8);
      bitIndex = bitIndex % 8;

      UInt64 mask = ~((~uInt64Mask[bitIndex]) & uInt64Mask[bitLength + bitIndex]);
      UInt64 longNumber = number;
      longNumber = longNumber << bitIndex;
      byte[] maskBytes = BitConverter.GetBytes(mask);
      byte[] numberBytes = BitConverter.GetBytes(longNumber);

      for (int index = 0; (index < 8) && ((byteIndex + index) < buffer.Length); index++)
      {
        byte maskByte = maskBytes[index];
        buffer[byteIndex + index] &= maskBytes[index];
        buffer[byteIndex + index] |= numberBytes[index];
      }
    }

    private const byte bitsPerInt = 32;
    private const byte bitsPerByte = 8;

    private static readonly System.UInt64[] uInt64Mask = 
		{
			0x00,  //  0 bits
			0x01,  //  1 bits
			0x03,  //  2 bits
			0x07,  //  3 bits
			0x0F,  //  4 bits
			0x1F,  //  5 bits
			0x3F,  //  6 bits
			0x7F,  //  7 bits
			0xFF,  //  8 bits
			0x1FF,  //  9 bits
			0x3FF,  // 10 bits
			0x7FF,  // 11 bits
			0xFFF,  // 12 bits
			0x1FFF,  // 13 bits
			0x3FFF,  // 14 bits
			0x7FFF,  // 15 bits
			0xFFFF,  // 16 bits
			0x1FFFF,  // 17 bits
			0x3FFFF,  // 18 bits
			0x7FFFF,  // 19 bits
			0xFFFFF,  // 20 bits
			0x1FFFFF,  // 21 bits
			0x3FFFFF,  // 22 bits
			0x7FFFFF,  // 23 bits
			0xFFFFFF,  // 24 bits
			0x1FFFFFF,  // 25 bits
			0x3FFFFFF,  // 26 bits
			0x7FFFFFF,  // 27 bits
			0xFFFFFFF,  // 28 bits
			0x1FFFFFFF,  // 29 bits
			0x3FFFFFFF,  // 30 bits
			0x7FFFFFFF,  // 31 bits
			0xFFFFFFFF,  // 32 bits
			0x1FFFFFFFF,        // 33
			0x3FFFFFFFF,        // 34
			0x7FFFFFFFF,        // 35
			0xFFFFFFFFF,        // 36
			0x1FFFFFFFFF,       // 37
			0x3FFFFFFFFF,       // 38
			0x7FFFFFFFFF,       // 39
			0xFFFFFFFFFF,       // 40
			0x1FFFFFFFFFF,      // 41
			0x3FFFFFFFFFF,      // 42
			0x7FFFFFFFFFF,      // 43
			0xFFFFFFFFFFF,      // 44
			0x1FFFFFFFFFFF,     // 45
			0x3FFFFFFFFFFF,     // 46
			0x7FFFFFFFFFFF,     // 47
			0xFFFFFFFFFFFF,     // 48
			0x1FFFFFFFFFFFF,    // 49
			0x3FFFFFFFFFFFF,    // 50
			0x7FFFFFFFFFFFF,    // 51
			0xFFFFFFFFFFFFF,    // 52
			0x1FFFFFFFFFFFFF,   // 53
			0x3FFFFFFFFFFFFF,   // 54
			0x7FFFFFFFFFFFFF,   // 55
			0xFFFFFFFFFFFFFF,   // 56
			0x1FFFFFFFFFFFFFF,  // 57
			0x3FFFFFFFFFFFFFF,  // 58
			0x7FFFFFFFFFFFFFF,  // 59
			0xFFFFFFFFFFFFFFF,  // 60
			0x1FFFFFFFFFFFFFFF, // 61
			0x3FFFFFFFFFFFFFFF, // 62
			0x7FFFFFFFFFFFFFFF, // 63
			0xFFFFFFFFFFFFFFFF  // 64 bits
		};
  }

}
