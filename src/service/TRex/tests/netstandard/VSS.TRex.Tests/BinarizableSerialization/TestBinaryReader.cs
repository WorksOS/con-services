using System;
using System.Collections;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Tests.BinarizableSerialization
{

  /// <summary>
  /// Binary reader implementation. 
  /// </summary>
  public class TestBinaryReader : IBinaryReader, IBinaryRawReader
  {
    private readonly BinaryReader br;

    public TestBinaryReader(MemoryStream ms)
    {
      ms.Position = 0;
      br = new BinaryReader(ms);
    }


    public IBinaryRawReader GetRawReader()
    {
      return this;
    }

    public bool ReadBoolean(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadBoolean(), BinaryTypeId.Bool);
    }

    public bool ReadBoolean()
    {
      return br.ReadBoolean();
    }

    public bool[] ReadBooleanArray(string fieldName)
    {
      throw new NotImplementedException();
      // return ReadField(fieldName, BinaryUtils.ReadBooleanArray, BinaryTypeId.ArrayBool);
    }

    public bool[] ReadBooleanArray()
    {
      return Read(r =>
      {
        if (!br.ReadBoolean()) // is null
          return null;

        var val = new bool[br.ReadInt32()];
        for (int i = 0; i < val.Length; i++)
          val[i] = br.ReadBoolean();
        return val;
      }, BinaryTypeId.ArrayBool);
    }

    public byte ReadByte(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadByte(), BinaryTypeId.Byte);
    }

    public byte ReadByte()
    {
      return br.ReadByte();
    }

    public byte[] ReadByteArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, ReadByteArray, BinaryTypeId.ArrayByte);
    }

    public byte[] ReadByteArray()
    {
      return Read(r => r.ReadBoolean() ? br.ReadBytes(br.ReadInt32()) : null, BinaryTypeId.ArrayByte);
    }

    public short ReadShort(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadInt16(), BinaryTypeId.Short);
    }

    public short ReadShort()
    {
      return br.ReadInt16();
    }

    public short[] ReadShortArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadShortArray, BinaryTypeId.ArrayShort);
    }

    public short[] ReadShortArray()
    {
      throw new NotImplementedException();
      //return Read(BinaryUtils.ReadShortArray, BinaryTypeId.ArrayShort);
    }

    public char ReadChar(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadChar(), BinaryTypeId.Char);
    }

    public char ReadChar()
    {
      return br.ReadChar();
    }

    public char[] ReadCharArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadCharArray, BinaryTypeId.ArrayChar);
    }

    public char[] ReadCharArray()
    {
      throw new NotImplementedException();
      //return Read(BinaryUtils.ReadCharArray, BinaryTypeId.ArrayChar);
    }

    public int ReadInt(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadInt32(), BinaryTypeId.Int);
    }

    public int ReadInt()
    {
      return br.ReadInt32();
    }

    public int[] ReadIntArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadIntArray, BinaryTypeId.ArrayInt);
    }

    public int[] ReadIntArray()
    {
      return Read(r =>
      {
        if (!br.ReadBoolean()) // is null
          return null;

        var val = new int[br.ReadInt32()];
        for (int i = 0; i < val.Length; i++)
          val[i] = br.ReadInt32();
        return val;
      }, BinaryTypeId.ArrayInt);
    }

    public long ReadLong(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadInt64(), BinaryTypeId.Long);
    }

    public long ReadLong()
    {
      return br.ReadInt64();
    }

    public long[] ReadLongArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadLongArray, BinaryTypeId.ArrayLong);
    }

    public long[] ReadLongArray()
    {
      return Read(r =>
      {
        if (!br.ReadBoolean()) // is null
          return null;

        var val = new long[br.ReadInt32()];
        for (int i = 0; i < val.Length; i++)
          val[i] = br.ReadInt64();
        return val;
      }, BinaryTypeId.ArrayLong);
    }

    public float ReadFloat(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadSingle(), BinaryTypeId.Float);
    }

    public float ReadFloat()
    {
      return br.ReadSingle();
    }

    public float[] ReadFloatArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadFloatArray, BinaryTypeId.ArrayFloat);
    }

    public float[] ReadFloatArray()
    {
      throw new NotImplementedException();
      //return Read(BinaryUtils.ReadFloatArray, BinaryTypeId.ArrayFloat);
    }

    public double ReadDouble(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadDouble(), BinaryTypeId.Double);
    }

    public double ReadDouble()
    {
      return br.ReadDouble();
    }

    public double[] ReadDoubleArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadDoubleArray, BinaryTypeId.ArrayDouble);
    }

    public double[] ReadDoubleArray()
    {
      return Read(r =>
      {
        if (!br.ReadBoolean()) // is null
          return null;

        var val = new double[br.ReadInt32()];
        for (int i = 0; i < val.Length; i++)
          val[i] = br.ReadDouble();
        return val;
      }, BinaryTypeId.ArrayDouble);
    }

    public decimal? ReadDecimal(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadDecimal, BinaryTypeId.Decimal);
    }

    public decimal? ReadDecimal()
    {
      throw new NotImplementedException();
      //return Read(BinaryUtils.ReadDecimal, BinaryTypeId.Decimal);
    }

    public decimal?[] ReadDecimalArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadDecimalArray, BinaryTypeId.ArrayDecimal);
    }

    public decimal?[] ReadDecimalArray()
    {
      throw new NotImplementedException();
      //return Read(BinaryUtils.ReadDecimalArray, BinaryTypeId.ArrayDecimal);
    }

    public DateTime? ReadTimestamp(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadTimestamp, BinaryTypeId.Timestamp);
    }

    public DateTime? ReadTimestamp()
    {
      throw new NotImplementedException();
      //return Read(BinaryUtils.ReadTimestamp, BinaryTypeId.Timestamp);
    }

    public DateTime?[] ReadTimestampArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, BinaryUtils.ReadTimestampArray, BinaryTypeId.ArrayTimestamp);
    }

    public DateTime?[] ReadTimestampArray()
    {
      throw new NotImplementedException();
      //return Read(BinaryUtils.ReadTimestampArray, BinaryTypeId.ArrayTimestamp);
    }

    public string ReadString(string fieldName)
    {
      return ReadField(fieldName, r => r.ReadString(), BinaryTypeId.String);
    }

    public string ReadString()
    {
      return Read(r => r.ReadBoolean() ? r.ReadString() : null, BinaryTypeId.String);
    }

    public string[] ReadStringArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, r => BinaryUtils.ReadArray<string>(r, false), BinaryTypeId.ArrayString);
    }

    public string[] ReadStringArray()
    {
      throw new NotImplementedException();
      //return Read(r => BinaryUtils.ReadArray<string>(r, false), BinaryTypeId.ArrayString);
    }

    public Guid? ReadGuid(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField<Guid?>(fieldName, r => BinaryUtils.ReadGuid(r), BinaryTypeId.Guid);
    }

    public Guid? ReadGuid()
    {
      return Read<Guid?>(r =>
      {
        if (r.ReadBoolean())
        {
          byte[] bytes = r.ReadBytes(16);
          return new Guid(bytes);
        }

        return null;
      }, BinaryTypeId.Guid);
    }

    public Guid?[] ReadGuidArray(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, r => BinaryUtils.ReadArray<Guid?>(r, false), BinaryTypeId.ArrayGuid);
    }

    public Guid?[] ReadGuidArray()
    {
      throw new NotImplementedException();
      //return Read(r => BinaryUtils.ReadArray<Guid?>(r, false), BinaryTypeId.ArrayGuid);
    }

    public T ReadEnum<T>(string fieldName)
    {
      throw new NotImplementedException();
      //return SeekField(fieldName) ? ReadEnum<T>() : default(T);
    }

    public T ReadEnum<T>()
    {
      throw new NotImplementedException();
    }

    public T[] ReadEnumArray<T>(string fieldName)
    {
      throw new NotImplementedException();
    }

    public T[] ReadEnumArray<T>()
    {
      throw new NotImplementedException();
    }

    public T ReadObject<T>(string fieldName)
    {
      throw new NotImplementedException();
    }

    public T ReadObject<T>()
    {
      return Deserialize<T>();
    }

    public T[] ReadArray<T>(string fieldName)
    {
      throw new NotImplementedException();
      //return ReadField(fieldName, r => BinaryUtils.ReadArray<T>(r, true), BinaryTypeId.Array);
    }

    public T[] ReadArray<T>()
    {
      throw new NotImplementedException();
      //return Read(r => BinaryUtils.ReadArray<T>(r, true), BinaryTypeId.Array);
    }

    public ICollection ReadCollection(string fieldName)
    {
      throw new NotImplementedException();
    }

    public ICollection ReadCollection()
    {
      return ReadCollection(null, null);
    }

    public ICollection ReadCollection(string fieldName, Func<int, ICollection> factory,
      Action<ICollection, object> adder)
    {
      throw new NotImplementedException();
    }

    public ICollection ReadCollection(Func<int, ICollection> factory, Action<ICollection, object> adder)
    {
      throw new NotImplementedException();
    }

    public IDictionary ReadDictionary(string fieldName)
    {
      return ReadDictionary(fieldName, null);
    }

    public IDictionary ReadDictionary()
    {
      return ReadDictionary((Func<int, IDictionary>) null);
    }

    public IDictionary ReadDictionary(string fieldName, Func<int, IDictionary> factory)
    {
      throw new NotImplementedException();
    }

    public IDictionary ReadDictionary(Func<int, IDictionary> factory)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Enable detach mode for the next object read. 
    /// </summary>
    public BinaryReader DetachNext()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Deserialize object.
    /// </summary>
    /// <param name="typeOverride">The type override.
    /// There can be multiple versions of the same type when peer assembly loading is enabled.
    /// Only first one is registered in Marshaller.
    /// This parameter specifies exact type to be instantiated.</param>
    /// <returns>Deserialized object.</returns>
    public T Deserialize<T>(Type typeOverride = null)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Deserialize object.
    /// </summary>
    /// <param name="res">Deserialized object.</param>
    /// <param name="typeOverride">The type override.
    /// There can be multiple versions of the same type when peer assembly loading is enabled.
    /// Only first one is registered in Marshaller.
    /// This parameter specifies exact type to be instantiated.</param>
    /// <returns>
    /// Deserialized object.
    /// </returns>
    public bool TryDeserialize<T>(out T res, Type typeOverride = null)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the flag indicating that there is custom type information in raw region.
    /// </summary>
    public bool GetCustomTypeDataFlag()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Reads the binary object.
    /// </summary>
    private T ReadBinaryObject<T>(bool doDetach)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Seeks specified field and invokes provided func.
    /// </summary>
    private T ReadField<T>(string fieldName, Func<BinaryReader, T> readFunc, byte expHdr)
    {
      return Read(readFunc, expHdr);
    }

    /// <summary>
    /// Reads header and invokes specified func if the header is not null.
    /// </summary>
    private T Read<T>(Func<BinaryReader, T> readFunc, byte expHdr)
    {
      var hdr = br.ReadByte();
      if (hdr != expHdr)
        throw new TRexException("Field header version incorrect on field read");

      return readFunc(br);
    }
  }
}
