using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace VSS.TRex.TAGFiles.Classes.Processors
{
  /// <summary>
  /// Implements TAG file reading semantics for TAG file data represented to it via a stream
  /// </summary>
  public class TAGReader
  {
    private const byte NYBBLE_COUNT = 4;

    // The stream provided in the constructor to read the TAG information from
    private Stream stream;

    /// <summary>
    /// The current nybble being read from the stream. 
    /// </summary>for(
    private byte nybble;

    /// <summary>
    /// The internal field for the position in the stream the reader is currently positioned at, in nybbles
    /// </summary>
    private long nybblePosition;

    /// <summary>
    /// The position in the stream the reader is currently positioned at, in nybbles.
    /// </summary>
    public long NybblePosition
    {
      get => nybblePosition;
      set => SetNybblePosition(value);
    }

    /// <summary>
    /// Default no-arg constructor. Do not use, this will throw an exception, use TagFileReader(Stream stream)
    /// </summary>
    public TAGReader()
    {
      throw new ArgumentException("Default TAGFileReader constructor is not permitted. Use TAGFileReader(Stream stream)");
    }

    /// <summary>
    /// TAG file reader constructor. Accepts a stream to read TAG data from.
    /// </summary>
    /// <param name="stream"></param>
    public TAGReader(Stream stream)
    {
      this.stream = stream;
      nybble = 0;
    }

    /// <summary>
    /// Retrieves the size of the stream in nybbles
    /// </summary>
    /// <returns></returns>
    public long GetSize() => stream.Length * 2;

    /// <summary>
    /// Read the next nybble from the stream
    /// </summary>
    /// <returns></returns>
    private byte ReadNybble()
    {
      byte result;

      if ((nybblePosition < 0) || (nybblePosition / 2 > GetSize()))
      {
        throw new IndexOutOfRangeException($"NybblePosition {nybblePosition} in file is out of range (size = {GetSize()})");
      }

      if (nybblePosition % 2 == 0)
      {
        nybble = (byte)stream.ReadByte();
      }

      switch (nybblePosition % 2)
      {
        case 0:
          result = (byte)((nybble >> NYBBLE_COUNT) & 0xf);
          break;
        case 1:
          result = (byte)(nybble & 0xf);
          break;

        default:
          result = 0;
          break;
      }

      nybblePosition++;

      return result;
    }


    /// <summary>
    /// Read an ANSI char from the stream. The result is returned as a byte as
    /// c# does not have a native ANSI type
    /// </summary>
    private byte /*ANSIChar*/ ReadANSIChar => (byte)ReadUnSignedIntegerValue(2);

    /// <summary>
    /// Read an ANSI string from the stream. The result is returned as a byte array as
    /// c# does not have a native ANSI type
    /// </summary>
    /// <returns></returns>
    public byte[] /*String*/ ReadANSIString()
    {
      byte b;
      int count = 0;
      byte[] result = new byte[100];

      while ((b = ReadANSIChar) != 0)
      {
        result[count++] = b;
        if (count == result.Length)
        {
          Array.Resize(ref result, result.Length + 100);
        }
      }

      Array.Resize(ref result, count);

      return result;
    }

    /// <summary>
    /// Read a buffer of count nybbles from the stream and return it as a byte[] array. 
    /// This method will only accept requests for an even number of nybbles to be read.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private byte[] ReadBuffer(uint count)
    {
      Debug.Assert(count % 2 != 1, "ReadBuffer can only read even number of nybbles");

      byte[] buffer = new byte[count / 2];

      for (int I = 0; I < (count / 2); I++)
      {
        buffer[I] = (byte)((ReadNybble() << NYBBLE_COUNT) | ReadNybble());
      }

      return buffer;
    }

    /// <summary>
    /// Read an IEEE double number from the stream
    /// </summary>
    /// <returns></returns>
    public double ReadDoublePrecisionIEEEValue() => BitConverter.ToDouble(ReadBuffer(16), 0);

    /// <summary>
    /// Read an IEEE single number from the stream
    /// </summary>
    /// <returns></returns>
    public float ReadSinglePrecisionIEEEValue() => BitConverter.ToSingle(ReadBuffer(8), 0);

    /// <summary>
    /// Read a 4, 8, 12, 16 or 32 bit signed integer from the stream.
    /// </summary>
    /// <param name="nNybbles"></param>
    /// <returns></returns>
    public int ReadSignedIntegerValue(byte nNybbles)
    {
      Debug.Assert(nNybbles > 0, "Nybble count < 0 in ReadSignedIntegerValue()");

      int firstNybble = ReadNybble();

      // Check to see if the number we are reading is negative and handle
      // accordingly since number in file is twos complement to match the memory
      // structure of the value when it was written in to the file.

      int result = -1;
      result = ((firstNybble & 0x08) != 0) ? (int)(result & 0xfffffff0) | firstNybble : firstNybble;

      for (int i = 1; i < nNybbles; i++)
      {
        result = (result << NYBBLE_COUNT) | ReadNybble();
      }

      return result;
    }

    /// <summary>
    /// Read a single UniCode character from the stream
    /// </summary>
    /// <returns></returns>
    private char ReadUnicodeChar() => BitConverter.ToChar(new byte[] { (byte)ReadUnSignedIntegerValue(NYBBLE_COUNT), 0 }, 0);

  /// <summary>
  /// Read a Unicode string from the stream
  /// </summary>
  /// <returns></returns>
  public string ReadUnicodeString()
    {
      char c;
      StringBuilder sb = new StringBuilder();

      while ((c = ReadUnicodeChar()) != char.MinValue)
      {
        sb.Append(c);
      }

      return sb.ToString();
    }

    /// <summary>
    /// Read a 4, 8, 12, 16 or 32 bit unsigned integer from the stream.
    /// </summary>
    /// <param name="nNybbles"></param>
    /// <returns></returns>
    public uint ReadUnSignedIntegerValue(byte nNybbles)
    {
      Debug.Assert(nNybbles > 0, "Nybble count < 0 in ReadSignedIntegerValue()");

      uint result = ReadNybble();

      for (int i = 1; i < nNybbles; i++)
      {
        result = (result << NYBBLE_COUNT) | ReadNybble();
      }

      return result;
    }

    /// <summary>
    /// Read a variable integer from the stream. A variable int uses a variable number of nybbles in te
    /// stream to define the integer with the number of nybbles designeted per the TAG file schema.
    /// </summary>
    /// <param name="varInt"></param>
    /// <returns></returns>
    public bool ReadVarInt(out short varInt)
    {
      varInt = 0;
      short firstNybble = ReadNybble();

      if ((firstNybble & 0x8) == 0x8)
      {
        varInt = (short)(firstNybble & 0x7);
        return true;
      }

      if ((firstNybble & 0xC) == 0x4) // There are two nybbles, value origin is 8
      {
        varInt = (short)(((short)((firstNybble & 0x3) << 4) | (short)ReadUnSignedIntegerValue(1)) + 8);
        return true;
      }

      if ((firstNybble & 0xE) == 0x2) // There are three nybbles, value origin is 72
      {
        varInt = (short)(((short)((firstNybble & 0x1) << 8) | (short)ReadUnSignedIntegerValue(2)) + 72);
        return true;
      }

      if (firstNybble == 1)  // There are four nybbles, value origin is 584, only
                             // the last three nybbles contain a value.
      {
        varInt = (short)(ReadUnSignedIntegerValue(3) + 584);
        return true;
      }

      // This is an invalid VarInt in the file.
      return false;
    }

    /// <summary>
    /// Sets the file stream Position location pointer based on the value in nybbles
    /// </summary>
    /// <param name="value"></param>
    private void SetNybblePosition(long value)
    {
      nybblePosition = value;

      stream.Position = value / 2;

      if (value % 2 == 1)
      {
        ReadNybble();
      }
    }
  }
}
