using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSS.Productivity3D.Models.Models.MapHandling
{
  public class DesignGeometryJsonConverter : JsonConverter
  {
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
      return typeof(Geometry).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      if (reader.TokenType == JsonToken.Null)
        return null;

      // Load JObject from stream
      var jObject = JObject.Load(reader);

      if (jObject == null)
        return null;

      string geometryType;

      if (Enum.TryParse(jObject.Value<string>("type"), true, out geometryType))
      {
        switch (geometryType)
        {
          case GeometryTypes.LINESTRING:
            var centerlineGeometry = new CenterlineGeometry();
            serializer.Populate(jObject.CreateReader(), centerlineGeometry);
            return centerlineGeometry;
          case GeometryTypes.POLYGON:
            var fenceGeometry = new FenceGeometry();
            serializer.Populate(jObject.CreateReader(), fenceGeometry);
            return fenceGeometry;
          default:
            throw new ArgumentException($"Unknown geometry type '{geometryType}'");
        }
      }

      throw new ArgumentException($"Unable to parse geometry object");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotImplementedException("Custom JSon converter is used for deserialization only.");
    }
  }
}
