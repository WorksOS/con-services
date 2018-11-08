using System;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Converters
{
  public class FormatLongAsStringConverter : JsonConverter
  {
    public override bool CanRead => false;
    public override bool CanWrite => true;
    public override bool CanConvert(Type type) => type == typeof(long);

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var number = (long) value;
      writer.WriteValue(number.ToString());
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
      throw new NotSupportedException();
    }
  }
}
