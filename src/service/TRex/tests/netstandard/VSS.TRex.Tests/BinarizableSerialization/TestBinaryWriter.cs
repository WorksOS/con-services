using System;
using System.Collections;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Extensions;

namespace VSS.TRex.Tests.BinarizableSerialization
{

  /// <summary>
  /// Binary writer implementation.
  /// </summary>
  public class TestBinaryWriter : IBinaryWriter, IBinaryRawWriter
  {
    /** Stream. */
    public readonly System.IO.BinaryWriter _stream = new System.IO.BinaryWriter(new MemoryStream());

    /// <summary>
    /// Write named boolean value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Boolean value.</param>
    public void WriteBoolean(string fieldName, bool val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Bool);
      WriteBooleanField(val);
    }

    /// <summary>
    /// Writes the boolean field.
    /// </summary>
    /// <param name="val">if set to <c>true</c> [value].</param>
    internal void WriteBooleanField(bool val)
    {
      _stream.Write(BinaryTypeId.Bool);
      _stream.Write(val);
    }

    /// <summary>
    /// Write boolean value.
    /// </summary>
    /// <param name="val">Boolean value.</param>
    public void WriteBoolean(bool val)
    {
      _stream.Write(val);
    }

    /// <summary>
    /// Write named boolean array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Boolean array.</param>
    public void WriteBooleanArray(string fieldName, bool[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayBool);

      _stream.Write(BinaryTypeId.ArrayBool);

      WriteBooleanArray(val);
    }

    /// <summary>
    /// Write boolean array.
    /// </summary>
    /// <param name="val">Boolean array.</param>
    public void WriteBooleanArray(bool[] val)
    {
      _stream.Write(BinaryTypeId.ArrayBool);

      _stream.Write(val != null);
      if (val != null)
      {
        _stream.Write(val.Length);

        val.ForEach(x => _stream.Write(x));
      }
    }

    /// <summary>
    /// Write named byte value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Byte value.</param>
    public void WriteByte(string fieldName, byte val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Byte);
      WriteByteField(val);
    }

    /// <summary>
    /// Write byte field value.
    /// </summary>
    /// <param name="val">Byte value.</param>
    internal void WriteByteField(byte val)
    {
      _stream.Write(BinaryTypeId.Byte);
      _stream.Write(val);
    }

    /// <summary>
    /// Write byte value.
    /// </summary>
    /// <param name="val">Byte value.</param>
    public void WriteByte(byte val)
    {
      _stream.Write(val);
    }

    /// <summary>
    /// Write named byte array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Byte array.</param>
    public void WriteByteArray(string fieldName, byte[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayByte);

      WriteByteArray(val);
    }

    /// <summary>
    /// Write byte array.
    /// </summary>
    /// <param name="val">Byte array.</param>
    public void WriteByteArray(byte[] val)
    {
      _stream.Write(BinaryTypeId.ArrayByte);

      _stream.Write(val != null);
      if (val != null)
      {
        _stream.Write(val.Length);
        _stream.Write(val);
      }
    }

    /// <summary>
    /// Write named short value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Short value.</param>
    public void WriteShort(string fieldName, short val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Short);
      WriteShortField(val);
    }

    /// <summary>
    /// Write short field value.
    /// </summary>
    /// <param name="val">Short value.</param>
    internal void WriteShortField(short val)
    {
      _stream.Write(BinaryTypeId.Short);
      _stream.Write(val);
    }

    /// <summary>
    /// Write short value.
    /// </summary>
    /// <param name="val">Short value.</param>
    public void WriteShort(short val)
    {
      _stream.Write(val);
    }

    /// <summary>
    /// Write named short array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Short array.</param>
    public void WriteShortArray(string fieldName, short[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayShort);

      _stream.Write(BinaryTypeId.ArrayShort);
      //BinaryUtils.WriteShortArray(val, _stream);
    }

    /// <summary>
    /// Write short array.
    /// </summary>
    /// <param name="val">Short array.</param>
    public void WriteShortArray(short[] val)
    {
      _stream.Write(BinaryTypeId.ArrayShort);
      throw new NotImplementedException();
      //BinaryUtils.WriteShortArray(val, _stream);
    }

    /// <summary>
    /// Write named char value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Char value.</param>
    public void WriteChar(string fieldName, char val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Char);
      WriteCharField(val);
    }

    /// <summary>
    /// Write char field value.
    /// </summary>
    /// <param name="val">Char value.</param>
    internal void WriteCharField(char val)
    {
      _stream.Write(BinaryTypeId.Char);
      _stream.Write(val);
    }

    /// <summary>
    /// Write char value.
    /// </summary>
    /// <param name="val">Char value.</param>
    public void WriteChar(char val)
    {
      _stream.Write(val);
    }

    /// <summary>
    /// Write named char array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Char array.</param>
    public void WriteCharArray(string fieldName, char[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayChar);

      _stream.Write(BinaryTypeId.ArrayChar);
      throw new NotImplementedException();
      //BinaryUtils.WriteCharArray(val, _stream);
    }

    /// <summary>
    /// Write char array.
    /// </summary>
    /// <param name="val">Char array.</param>
    public void WriteCharArray(char[] val)
    {
      _stream.Write(BinaryTypeId.ArrayChar);
      throw new NotImplementedException();
      //BinaryUtils.WriteCharArray(val, _stream);
    }

    /// <summary>
    /// Write named int value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Int value.</param>
    public void WriteInt(string fieldName, int val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Int);
      WriteIntField(val);
    }

    /// <summary>
    /// Writes the int field.
    /// </summary>
    /// <param name="val">The value.</param>
    internal void WriteIntField(int val)
    {
      _stream.Write(BinaryTypeId.Int);
      _stream.Write(val);
    }

    /// <summary>
    /// Write int value.
    /// </summary>
    /// <param name="val">Int value.</param>
    public void WriteInt(int val)
    {
      _stream.Write(val);
    }

    /// <summary>
    /// Write named int array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Int array.</param>
    public void WriteIntArray(string fieldName, int[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayInt);
      WriteIntArray(val);
    }

    /// <summary>
    /// Write int array.
    /// </summary>
    /// <param name="val">Int array.</param>
    public void WriteIntArray(int[] val)
    {
      _stream.Write(BinaryTypeId.ArrayInt);

      _stream.Write(val != null);
      if (val != null)
      {
        _stream.Write(val.Length);
        val.ForEach(item => _stream.Write(item));
      }
    }

    /// <summary>
    /// Write named long value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Long value.</param>
    public void WriteLong(string fieldName, long val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Long);
      WriteLongField(val);
    }

    /// <summary>
    /// Writes the long field.
    /// </summary>
    /// <param name="val">The value.</param>
    internal void WriteLongField(long val)
    {
      _stream.Write(BinaryTypeId.Long);
      _stream.Write(val);
    }

    /// <summary>
    /// Write long value.
    /// </summary>
    /// <param name="val">Long value.</param>
    public void WriteLong(long val)
    {
      _stream.Write(val);
    }

    /// <summary>
    /// Write named long array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Long array.</param>
    public void WriteLongArray(string fieldName, long[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayLong);

      WriteLongArray(val);
    }

    /// <summary>
    /// Write long array.
    /// </summary>
    /// <param name="val">Long array.</param>
    public void WriteLongArray(long[] val)
    {
      _stream.Write(BinaryTypeId.ArrayLong);

      _stream.Write(val != null);
      if (val != null)
      {
        _stream.Write(val.Length);
        val.ForEach(item => _stream.Write(item));
      }
    }

    /// <summary>
    /// Write named float value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Float value.</param>
    public void WriteFloat(string fieldName, float val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Float);
      WriteFloatField(val);
    }

    /// <summary>
    /// Writes the float field.
    /// </summary>
    /// <param name="val">The value.</param>
    internal void WriteFloatField(float val)
    {
      _stream.Write(BinaryTypeId.Float);
      _stream.Write(val);
    }

    /// <summary>
    /// Write float value.
    /// </summary>
    /// <param name="val">Float value.</param>
    public void WriteFloat(float val)
    {
      _stream.Write(val);
    }

    /// <summary>
    /// Write named float array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Float array.</param>
    public void WriteFloatArray(string fieldName, float[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayFloat);

      _stream.Write(BinaryTypeId.ArrayFloat);
      throw new NotImplementedException();
      //BinaryUtils.WriteFloatArray(val, _stream);
    }

    /// <summary>
    /// Write float array.
    /// </summary>
    /// <param name="val">Float array.</param>
    public void WriteFloatArray(float[] val)
    {
      _stream.Write(BinaryTypeId.ArrayFloat);
      throw new NotImplementedException();
      //BinaryUtils.WriteFloatArray(val, _stream);
    }

    /// <summary>
    /// Write named double value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Double value.</param>
    public void WriteDouble(string fieldName, double val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Double);
      WriteDoubleField(val);
    }

    /// <summary>
    /// Writes the double field.
    /// </summary>
    /// <param name="val">The value.</param>
    internal void WriteDoubleField(double val)
    {
      _stream.Write(BinaryTypeId.Double);
      _stream.Write(val);
    }

    /// <summary>
    /// Write double value.
    /// </summary>
    /// <param name="val">Double value.</param>
    public void WriteDouble(double val)
    {
      _stream.Write(val);
    }

    /// <summary>
    /// Write named double array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Double array.</param>
    public void WriteDoubleArray(string fieldName, double[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayDouble);

      WriteDoubleArray(val);
    }

    /// <summary>
    /// Write double array.
    /// </summary>
    /// <param name="val">Double array.</param>
    public void WriteDoubleArray(double[] val)
    {
      _stream.Write(BinaryTypeId.ArrayDouble);

      _stream.Write(val != null);
      if (val != null)
      {
        _stream.Write(val.Length);
        val.ForEach(item => _stream.Write(item));
      }
    }

    /// <summary>
    /// Write named decimal value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Decimal value.</param>
    public void WriteDecimal(string fieldName, decimal? val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write decimal value.
    /// </summary>
    /// <param name="val">Decimal value.</param>
    public void WriteDecimal(decimal? val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named decimal array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Decimal array.</param>
    public void WriteDecimalArray(string fieldName, decimal?[] val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write decimal array.
    /// </summary>
    /// <param name="val">Decimal array.</param>
    public void WriteDecimalArray(decimal?[] val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named date value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Date value.</param>
    public void WriteTimestamp(string fieldName, DateTime? val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write date value.
    /// </summary>
    /// <param name="val">Date value.</param>
    public void WriteTimestamp(DateTime? val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named date array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Date array.</param>
    public void WriteTimestampArray(string fieldName, DateTime?[] val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write date array.
    /// </summary>
    /// <param name="val">Date array.</param>
    public void WriteTimestampArray(DateTime?[] val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named string value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">String value.</param>
    public void WriteString(string fieldName, string val)
    {
      WriteFieldId(fieldName, BinaryTypeId.String);

      _stream.Write(BinaryTypeId.String);
      _stream.Write(val);
    }

    /// <summary>
    /// Write string value.
    /// </summary>
    /// <param name="val">String value.</param>
    public void WriteString(string val)
    {
      _stream.Write(BinaryTypeId.String);

      _stream.Write(val != null);
      if (val != null)
        _stream.Write(val);
    }

    /// <summary>
    /// Write named string array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">String array.</param>
    public void WriteStringArray(string fieldName, string[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayString);

      _stream.Write(BinaryTypeId.ArrayString);
      throw new NotImplementedException();
      //BinaryUtils.WriteStringArray(val, _stream);
    }

    /// <summary>
    /// Write string array.
    /// </summary>
    /// <param name="val">String array.</param>
    public void WriteStringArray(string[] val)
    {
      _stream.Write(BinaryTypeId.ArrayString);
      throw new NotImplementedException();
      //BinaryUtils.WriteStringArray(val, _stream);
    }

    /// <summary>
    /// Write named GUID value.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">GUID value.</param>
    public void WriteGuid(string fieldName, Guid? val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Guid);

      WriteGuid(val);
    }

    /// <summary>
    /// Write GUID value.
    /// </summary>
    /// <param name="val">GUID value.</param>
    public void WriteGuid(Guid? val)
    {
      _stream.Write(BinaryTypeId.Guid);

      _stream.Write(val != null);
      if (val != null)
        _stream.Write(val.Value.ToByteArray());
    }

    /// <summary>
    /// Write named GUID array.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">GUID array.</param>
    public void WriteGuidArray(string fieldName, Guid?[] val)
    {
      WriteFieldId(fieldName, BinaryTypeId.ArrayGuid);

      _stream.Write(BinaryTypeId.ArrayGuid);
      throw new NotImplementedException();
      //BinaryUtils.WriteGuidArray(val, _stream);
    }

    /// <summary>
    /// Write GUID array.
    /// </summary>
    /// <param name="val">GUID array.</param>
    public void WriteGuidArray(Guid?[] val)
    {
      _stream.Write(BinaryTypeId.ArrayGuid);
      throw new NotImplementedException();
      //BinaryUtils.WriteGuidArray(val, _stream);
    }

    /// <summary>
    /// Write named enum value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Enum value.</param>
    public void WriteEnum<T>(string fieldName, T val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write enum value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val">Enum value.</param>
    public void WriteEnum<T>(T val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write enum value.
    /// </summary>
    /// <param name="val">Enum value.</param>
    /// <param name="type">Enum type.</param>
    internal void WriteEnum(int val, Type type)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named enum array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Enum array.</param>
    public void WriteEnumArray<T>(string fieldName, T[] val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write enum array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val">Enum array.</param>
    public void WriteEnumArray<T>(T[] val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Writes the enum array.
    /// </summary>
    /// <param name="val">The value.</param>
    /// <param name="elementTypeId">The element type id.</param>
    public void WriteEnumArrayInternal(Array val, int? elementTypeId)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named object value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Object value.</param>
    public void WriteObject<T>(string fieldName, T val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write object value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val">Object value.</param>
    public void WriteObject<T>(T val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named object array.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Object array.</param>
    public void WriteArray<T>(string fieldName, T[] val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write object array.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="val">Object array.</param>
    public void WriteArray<T>(T[] val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write object array.
    /// </summary>
    /// <param name="val">Object array.</param>
    public void WriteArrayInternal(Array val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named collection.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Collection.</param>
    public void WriteCollection(string fieldName, ICollection val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Collection);

      WriteCollection(val);
    }

    /// <summary>
    /// Write collection.
    /// </summary>
    /// <param name="val">Collection.</param>
    public void WriteCollection(ICollection val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write named dictionary.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="val">Dictionary.</param>
    public void WriteDictionary(string fieldName, IDictionary val)
    {
      WriteFieldId(fieldName, BinaryTypeId.Dictionary);

      WriteDictionary(val);
    }

    /// <summary>
    /// Write dictionary.
    /// </summary>
    /// <param name="val">Dictionary.</param>
    public void WriteDictionary(IDictionary val)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get raw writer.
    /// </summary>
    /// <returns>
    /// Raw writer.
    /// </returns>
    public IBinaryRawWriter GetRawWriter()
    {
      return this;
    }

    /// <summary>
    /// Write object.
    /// </summary>
    /// <param name="obj">Object.</param>
    public void Write<T>(T obj)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Marks current object with a custom type data flag.
    /// </summary>
    public void SetCustomTypeDataFlag(bool hasCustomTypeData)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Write primitive type.
    /// </summary>
    /// <param name="val">Object.</param>
    /// <param name="type">Type.</param>
    private void WritePrimitive<T>(T val, Type type)
    {
    }

    /// <summary>
    /// Try writing object as special builder type.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns>True if object was written, false otherwise.</returns>
    private bool WriteBuilderSpecials<T>(T obj)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Add handle to handles map.
    /// </summary>
    /// <param name="pos">Position in stream.</param>
    /// <param name="obj">Object.</param>
    /// <returns><c>true</c> if object was written as handle.</returns>
    private bool WriteHandle(long pos, object obj)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Perform action with detached semantics.
    /// </summary>
    internal void WriteObjectDetached<T>(T o)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets or sets a function to wrap all serializer objects.
    /// </summary>
    internal Func<object, object> WrapperFunc { get; set; }

    /// <summary>
    /// Write field ID.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="fieldTypeId">Field type ID.</param>
    private void WriteFieldId(string fieldName, byte fieldTypeId)
    {
      throw new NotImplementedException();
    }

  }
}
