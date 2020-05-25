using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VSS.Common.Abstractions.Enums
{
  public class NullableEnumStringConverter : StringEnumConverter
  {
    private readonly object _defaultValue;

    public NullableEnumStringConverter(object defaultValue)
    {
      _defaultValue = defaultValue ?? 0;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      if (reader.Value == null)
        return _defaultValue;
      return base.ReadJson(reader, objectType, existingValue, serializer);
    }
  }
}
