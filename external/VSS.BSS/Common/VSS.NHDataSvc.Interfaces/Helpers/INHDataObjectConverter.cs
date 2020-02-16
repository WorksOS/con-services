using System;
using Newtonsoft.Json;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHDataSvc.Interfaces.Helpers
{
  public class INHDataObjectConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("clrType");
      writer.WriteValue(value.GetType().AssemblyQualifiedName);
      writer.WritePropertyName("value");
      writer.WriteRawValue(JsonConvert.SerializeObject(value));
      writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      reader.Read(); // CrlType prop name
      reader.Read(); // actual type

      var type = Type.GetType((string)reader.Value);

      reader.Read(); // value property
      reader.Read(); // actual value

      var target = Activator.CreateInstance(type);
      serializer.Populate(reader, target);

      if(reader.TokenType == JsonToken.EndObject)
        reader.Read();

      return target;
    }

    public override bool CanConvert(Type objectType)
    {
      throw new NotImplementedException();
    }
  }
}
