using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Converters
{
  /// <summary>
  /// Used in JSON Serialization.
  /// Converts 'Magic' numbers into null values in JSON
  /// Defaults to MaxValue for numbers, and min value for DateTime
  /// But can be overridden by passing a value into the attribute
  /// </summary>
  public class RaptorNullableConverter : JsonConverter
  {
    private readonly object _overrideNullValue;

    private readonly Dictionary<Type, object> _nullValues = new Dictionary<Type, object>
    {
      {typeof(sbyte), sbyte.MaxValue},
      {typeof(byte), byte.MaxValue},
      {typeof(short), short.MaxValue},
      {typeof(ushort), ushort.MaxValue},
      {typeof(int), int.MaxValue},
      {typeof(uint), uint.MaxValue},
      {typeof(long), long.MaxValue},
      {typeof(ulong), ulong.MaxValue},
      {typeof(float), float.MaxValue},
      {typeof(DateTime), DateTime.MinValue}
    };

    public RaptorNullableConverter()
    {
      _overrideNullValue = null;
    }

    public RaptorNullableConverter(object overrideNullValue)
    {
      _overrideNullValue = overrideNullValue;
    }

    public override bool CanRead
    {
      get { return false; }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var type = value.GetType();

      if (_nullValues.TryGetValue(type, out var v) && value.Equals(v))
      {
        writer.WriteNull();
      }
      else if (_overrideNullValue != null)
      {
        // This must be convertible, or an exception will be thrown
        // I think it's better to let the exception bubble up than handle here, and send potentially misleading data
        var convertedValue = Convert.ChangeType(_overrideNullValue, type);
        if(value.Equals(convertedValue))
          writer.WriteNull();
        else
          writer.WriteValue(value);
      }
      else
      {
        writer.WriteValue(value);
      }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      throw new NotSupportedException();
    }

    public override bool CanConvert(Type objectType)
    {
      return _nullValues.ContainsKey(objectType);
    }
  }
}
