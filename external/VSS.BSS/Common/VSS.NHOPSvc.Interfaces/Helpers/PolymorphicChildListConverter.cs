using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using System.Text;
using Newtonsoft.Json;
using VSS.Nighthawk.MassTransit;
using System.Collections;

namespace VSS.Nighthawk.NHOPSvc.Interfaces.Helpers
{
  public class PolymorphicChildListConverter : JsonConverter
  {
    private readonly Type _explicitListType;

    public PolymorphicChildListConverter()
    {
    }

    protected PolymorphicChildListConverter(Type type)
    {
      _explicitListType = type;
    }

    public override void WriteJson(JsonWriter writer, object value,
      JsonSerializer serializer)
    {
      writer.WriteStartArray();
      foreach (var item in (IEnumerable)value)
      {
        writer.WriteStartObject();

        writer.WritePropertyName("CrlType");
        writer.WriteValue(item.GetType().AssemblyQualifiedName);

        writer.WritePropertyName("Value");
        serializer.Serialize(writer, item);

        writer.WriteEndObject();
      }
      writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader,
        Type objectType, object existingValue,
        JsonSerializer serializer)
    {
      var list = (IList)Activator.CreateInstance(
        _explicitListType ?? objectType);

      while (reader.Read())
      {
        if (reader.TokenType == JsonToken.EndArray)
          break;

        reader.Read(); //CrlType prop name
        reader.Read(); //actual type
        var type = Type.GetType((string)reader.Value);
        reader.Read(); // value property
        reader.Read(); // actual value
        list.Add(serializer.Deserialize(reader, type));
        reader.Read(); // end object
      }
      return list;
    }

    public override bool CanConvert(Type objectType)
    {
      var deserializationType = (_explicitListType ?? objectType);
      return typeof(IEnumerable).IsAssignableFrom(objectType) &&
             deserializationType.IsClass &&
             !deserializationType.IsAbstract;
    }
  }
}