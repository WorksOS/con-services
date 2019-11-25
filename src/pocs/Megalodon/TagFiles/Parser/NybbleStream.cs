using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TagFiles.Parser
{
  public class NybbleStream : IDisposable
  {

    // Flag: Has Dispose already been called?
    bool disposed = false;
    // Instantiate a SafeHandle instance.
    SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

    private const byte BITS_PER_NYBBLE = 4;
    private const byte BITS_PER_TWO_NYBBLES = 8;
    private const byte NYBBLES_PER_BYTE = 2;
    public bool HighNybble;
    private byte CurrentByte;
    private byte[] readANSIString_ByteBuffer = null;
    public Stream stream;

    // The size of the stream in nybbles
    public long StreamSizeInNybbles;
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
    /// Make sure we end content on a full padded byte
    /// </summary>
    public void Pad()
    {
      if (HighNybble)
        WriteNybble(0);
    }

    public void Position(long pos)
    {
      stream.Position = pos;
    }

    /// <summary>
    /// Read the next nybble from the stream
    /// </summary>
    /// <returns></returns>
    public byte ReadNybble()
    {
      byte result;
      if (HighNybble)
        result = (byte)(CurrentByte & 0xF); // already read so return left side
      else
      {
        CurrentByte = (byte)stream.ReadByte(); // read byte
        if ((int)CurrentByte == -1)
          throw new IndexOutOfRangeException($"NybblePosition out of range.");
        result = (byte)(CurrentByte >> 4); // return right side
      }
      HighNybble = !HighNybble;
      return result;
    }

    public void WriteNybble(byte data)
    {
      // 100 is 01100100 in binary
      // CurrentByte & 0xF0 = first(left) 4 bits = 01100000
      // CurrentByte & 0x0F = last(right) 4 bits = 00000100

      if (HighNybble) // left side
      {
        CurrentByte = (byte)((CurrentByte & 0xF0) | (data & 0x0F));
        stream.Seek(stream.Position - 1, 0); // overwrite same byte
        stream.WriteByte(CurrentByte);
      }
      else // right side
      {
        CurrentByte = (byte)((CurrentByte & 0x0F) | (data << 4));
        stream.WriteByte(CurrentByte);
      }

      HighNybble = ! HighNybble;
    }

    public void WriteFixedSizeUnsignedInt(uint data, byte nNybbles)
    {
      var nybbleCount = (nNybbles * 4) - 4;
      while (nybbleCount >= 0)
      {
        WriteNybble((byte)((data >> nybbleCount) & 0x0F));
        nybbleCount = nybbleCount - 4;
      } 
    }


    public void WriteVarSizeUnsignedInt(uint data, byte nNybbles)
    {

      switch (nNybbles)
      {
        case 1:
          WriteNybble((byte)(0x08 | (data & 0x07)));
          break;
        case 2:
          WriteNybble((byte)(0x04 | ((data >> 4) & 0x03)));
          WriteNybble((byte)(data & 0x0F));
          break;
        case 3:
          WriteNybble((byte)(0x02 | ((data >> 8) & 0x01)));
          WriteNybble((byte)((data >> 4) & 0x0F));
          WriteNybble((byte)(data & 0x0F));
          break;
        case 4:
          WriteNybble(0x01);
          WriteNybble((byte)((data >> 8) & 0x0F));
          WriteNybble((byte)((data >> 4) & 0x0F));
          WriteNybble((byte)(data & 0x0F));
          break;
        default:
          throw new InvalidOperationException("Variable sized unsigned integer cannot have more than 4 nybbles");
      }
    }

    public void WriteVarSizeUnsignedInt(uint data)
    {
      if (data <= 7)
        WriteVarSizeUnsignedInt(data, 1);
      else if (data <= 71)
        WriteVarSizeUnsignedInt(data - 8, 2);
      else if (data <= 583)
        WriteVarSizeUnsignedInt(data - 72, 3);
      else if (data <= 4679)
        WriteVarSizeUnsignedInt(data - 584, 4);
      else
        throw new InvalidOperationException("Variable sized unsigned integer cannot be greater than 4679");
    }


    public long Length()
    {
      return stream.Length;
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
    /// Buffer to be used by ReadSinglePrecisionIEEEValue()
    /// </summary>
    private byte[] ReadSinglePrecisionIEEEValue_Buffer = new byte[100];//GenericArrayPoolCacheHelper<byte>.Caches().Rent(4);


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
    /// Buffer to be used by ReadDoublePrecisionIEEEValue()
    /// </summary>
    private byte[] ReadDoublePrecisionIEEEValue_Buffer = new byte[100]; // GenericArrayPoolCacheHelper<byte>.Caches().Rent(8);

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

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
      if (disposed)
        return;

      if (disposing)
      {
        handle.Dispose();
        if (stream != null)
          stream.Close();
        // Free any other managed objects here.
        //
      }

      disposed = true;
    }

    /// <summary>
    /// Read an ANSI string from the stream. The result is returned as a byte array as
    /// c# does not have a native ANSI type
    /// </summary>
    /// <returns></returns>
    public string ReadANSIString()
    {
      byte b;
      int count = 0;
      if (readANSIString_ByteBuffer == null)
      {
        readANSIString_ByteBuffer = new byte[100]; // todo
      }

      while ((b = (byte)((ReadNybble() << BITS_PER_NYBBLE) | ReadNybble())) != 0)
      {
        readANSIString_ByteBuffer[count++] = b;
        if (count == readANSIString_ByteBuffer.Length)
          Array.Resize(ref readANSIString_ByteBuffer, readANSIString_ByteBuffer.Length + 100);
      }

      return Encoding.ASCII.GetString(readANSIString_ByteBuffer, 0, count);
    }

    /// <summary>
    /// Internal buffer used for ReadUnicodeCHar operations
    /// </summary>
    private readonly byte[] ReadUnicodeChar_ByteBuffer = { 0, 0 };

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

    private char[] _readUnicodeString_Buffer = new char[300];// GenericArrayPoolCacheHelper<char>.Caches().Rent(100);

    /// <summary>
    /// The byte buffer for reading bytes representing an ANSI string before construction of the string itself
    /// </summary>
    private byte[] _readANSIString_ByteBuffer = new byte[300];// GenericArrayPoolCacheHelper<byte>.Caches().Rent(100);

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
  }
}
