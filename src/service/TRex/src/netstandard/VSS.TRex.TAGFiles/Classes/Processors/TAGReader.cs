using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.TAGFiles.Classes.Processors
{
  /// <summary>
  /// Implements TAG file reading semantics for TAG file data represented to it via a stream
  /// </summary>
  public class TAGReader : IDisposable
  {
    private const byte BITS_PER_NYBBLE = 4;
    private const byte BITS_PER_TWO_NYBBLES = 8;
    private const byte NYBBLES_PER_BYTE = 2;

    // The stream provided in the constructor to read the TAG information from
    private readonly Stream stream;

    // The size of the stream in nybbles
    public readonly long StreamSizeInNybbles;

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
    /// TAG file reader constructor. Accepts a stream to read TAG data from.
    /// </summary>
    /// <param name="stream"></param>
    public TAGReader(Stream stream)
    {
      this.stream = stream;
      nybble = 0;
      StreamSizeInNybbles = stream.Length * 2;
    }

    /// <summary>
    /// Read the next nybble from the stream
    /// </summary>
    /// <returns></returns>
    private byte ReadNybble()
    {
      if (nybblePosition < 0 || nybblePosition / 2 > StreamSizeInNybbles)
        throw new IndexOutOfRangeException($"NybblePosition {nybblePosition} in file is out of range (size = {StreamSizeInNybbles})");

      if (nybblePosition++ % 2 == 0)
      {
        nybble = (byte) stream.ReadByte();
        return (byte)(nybble >> BITS_PER_NYBBLE);
      }

      return (byte)(nybble & 0xf);
    }

    /// <summary>
    /// Read an ANSI char from the stream. The result is returned as a byte as
    /// c# does not have a native ANSI type
    /// </summary>
    private byte ReadANSIChar => (byte)((ReadNybble() << BITS_PER_NYBBLE) | ReadNybble()); //(byte)ReadUnSignedIntegerValue(2);

    /// <summary>
    /// The byte buffer for reading bytes representing an ANSI string before construction of the string itself
    /// </summary>
    private byte[] _readANSIString_ByteBuffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(100);

    /// <summary>
    /// Read an ANSI string from the stream. The result is returned as a byte array as
    /// c# does not have a native ANSI type
    /// </summary>
    /// <returns></returns>
    public string ReadANSIString()
    {
      byte b;
      int count = 0;

      //while ((b = ReadANSIChar) != 0)
      while ((b = (byte)((ReadNybble() << BITS_PER_NYBBLE) | ReadNybble())) != 0)
      {
        _readANSIString_ByteBuffer[count++] = b;
        if (count == _readANSIString_ByteBuffer.Length)
          Array.Resize(ref _readANSIString_ByteBuffer, _readANSIString_ByteBuffer.Length + 100);
      }

      return Encoding.ASCII.GetString(_readANSIString_ByteBuffer, 0, count);
    }

    /// <summary>
    /// Read a buffer of count nybbles from the stream and return it as a byte[] array. 
    /// This method will only accept requests for an even number of nybbles to be read.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    private void ReadBufferEx(uint count, byte[] buffer)
    {
      Debug.Assert(count % 2 != 1, "ReadBuffer can only read even number of nybbles");

      for (int I = 0; I < (count / 2); I++)
        buffer[I] = (byte)((ReadNybble() << BITS_PER_NYBBLE) | ReadNybble());
    }

    /// <summary>
    /// Buffer to be used by ReadDoublePrecisionIEEEValue()
    /// </summary>
    private byte[] ReadDoublePrecisionIEEEValue_Buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(8);

    /// <summary>
    /// Read an IEEE double number from the stream
    /// </summary>
    /// <returns></returns>
    public double ReadDoublePrecisionIEEEValue()
    {
      ReadBufferEx(16, ReadDoublePrecisionIEEEValue_Buffer);
      return BitConverter.ToDouble(ReadDoublePrecisionIEEEValue_Buffer, 0);
    }

    /// <summary>
    /// Buffer to be used by ReadSinglePrecisionIEEEValue()
    /// </summary>
    private byte[] ReadSinglePrecisionIEEEValue_Buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(4);

    /// <summary>
    /// Read an IEEE single number from the stream
    /// </summary>
    /// <returns></returns>
    public float ReadSinglePrecisionIEEEValue()
    {
      ReadBufferEx(8, ReadSinglePrecisionIEEEValue_Buffer);
      return BitConverter.ToSingle(ReadSinglePrecisionIEEEValue_Buffer, 0);
    }

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
      result = (firstNybble & 0x08) != 0 ? (int)(result & 0xfffffff0) | firstNybble : firstNybble;

      for (int i = 1; i < nNybbles; i++)
        result = (result << BITS_PER_NYBBLE) | ReadNybble();

      return result;
    }

    /// <summary>
    /// Internal buffer used for ReadUnicodeCHar operations
    /// </summary>
    private readonly byte[] ReadUnicodeChar_ByteBuffer = {0, 0};

    /// <summary>
    /// Read a single UniCode character from the stream
    /// </summary>
    /// <returns></returns>
    private char ReadUnicodeChar()
    {
      ReadUnicodeChar_ByteBuffer[1] = (byte)ReadUnSignedIntegerValue(NYBBLES_PER_BYTE);
      ReadUnicodeChar_ByteBuffer[0] = (byte)ReadUnSignedIntegerValue(NYBBLES_PER_BYTE);
      return BitConverter.ToChar(ReadUnicodeChar_ByteBuffer, 0);
    }

    private char[] _readUnicodeString_Buffer = GenericArrayPoolCacheHelper<char>.Caches().Rent(100);

    /// <summary>
    /// Read a Unicode string from the stream
    /// </summary>
    /// <returns></returns>
    public string ReadUnicodeString()
    {
      char c;
      int count = 0;

      int bufferSize = _readANSIString_ByteBuffer.Length;
      while ((c = ReadUnicodeChar()) != char.MinValue)
      {
        _readUnicodeString_Buffer[count++] = c;

        if (count >= bufferSize)
        {
          Array.Resize(ref _readUnicodeString_Buffer, bufferSize + 100);
          bufferSize = _readANSIString_ByteBuffer.Length;
        }
      }

      return new string(_readUnicodeString_Buffer, 0, count);
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
        result = (result << BITS_PER_NYBBLE) | ReadNybble();
      }

      return result;
    }

    /// <summary>
    /// Read a variable integer from the stream. A variable int uses a variable number of nybbles in te
    /// stream to define the integer with the number of nybbles designated per the TAG file schema.
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
        varInt = (short)(((short)((firstNybble & 0x3) << BITS_PER_NYBBLE) | (short)ReadNybble()) + 8);
        return true;
      }

      if ((firstNybble & 0xE) == 0x2) // There are three nybbles, value origin is 72
      {
        varInt = (short)(((short)((firstNybble & 0x1) << BITS_PER_TWO_NYBBLES) | (short)((ReadNybble() << BITS_PER_NYBBLE) | ReadNybble())) + 72);
        return true;
      }

      if (firstNybble == 1)  // There are four nybbles, value origin is 584, only
                             // the last three nybbles contain a value.
      {
        varInt = (short)((ReadNybble() << BITS_PER_TWO_NYBBLES) | ((ReadNybble() << BITS_PER_NYBBLE) | ReadNybble()) + 584);
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
        ReadNybble();
    }

    #region IDisposable Support
    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          GenericArrayPoolCacheHelper<byte>.Caches().Return(ref ReadDoublePrecisionIEEEValue_Buffer);
          GenericArrayPoolCacheHelper<byte>.Caches().Return(ref ReadSinglePrecisionIEEEValue_Buffer);
          GenericArrayPoolCacheHelper<char>.Caches().Return(ref _readUnicodeString_Buffer);
          GenericArrayPoolCacheHelper<byte>.Caches().Return(ref _readANSIString_ByteBuffer);
        }

        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }
}
