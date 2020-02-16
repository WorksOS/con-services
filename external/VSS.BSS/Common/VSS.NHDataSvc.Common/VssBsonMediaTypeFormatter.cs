using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Formatting;
using System.Net.Http;
using System;
using System.Net;
using Newtonsoft.Json.Bson;
using System.IO;
using System.Collections;

namespace VSS.Nighthawk.NHDataSvc.Common
{
  public class VssBsonMediaTypeFormatter : MediaTypeFormatter
  {
    private JsonSerializerSettings _jsonSerializerSettings;

    public VssBsonMediaTypeFormatter()
    {
      SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/bson"));
      _jsonSerializerSettings = CreateDefaultSerializerSettings();
    }

    public JsonSerializerSettings SerializerSettings
    {
      get { return _jsonSerializerSettings; }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("Value is null");
        }

        _jsonSerializerSettings = value;
      }
    }

    public JsonSerializerSettings CreateDefaultSerializerSettings()
    {
      return new JsonSerializerSettings()
      {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None
      };
    }

    public override bool CanReadType(Type type)
    {
      if (type == null) throw new ArgumentNullException("type is null");
      return true;
    }

    public override bool CanWriteType(Type type)
    {
      if (type == null) throw new ArgumentNullException("Type is null");
      return true;
    }

    public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
    {
      var tcs = new TaskCompletionSource<object>();
      if (content.Headers != null && content.Headers.ContentLength == 0) return null;

      try
      {
        BsonReader reader = new BsonReader(readStream);

        if (typeof(IEnumerable).IsAssignableFrom(type)) reader.ReadRootValueAsArray = true;

        using (reader)
        {
          var jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings);
          var output = jsonSerializer.Deserialize(reader, type);
          if (formatterLogger != null)
          {
            jsonSerializer.Error += (sender, e) =>
            {
              Exception exception = e.ErrorContext.Error;
              formatterLogger.LogError(e.ErrorContext.Path, exception.Message);
              e.ErrorContext.Handled = true;
            };
          }
          tcs.SetResult(output);
        }
      }
      catch (Exception e)
      {
        if (formatterLogger == null) throw;
        formatterLogger.LogError(String.Empty, e.Message);
        tcs.SetResult(GetDefaultValueForType(type));
      }

      return tcs.Task;
    }

    public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
    {
      if (type == null) throw new ArgumentNullException("type is null");
      if (writeStream == null) throw new ArgumentNullException("Write stream is null");

      var tcs = new TaskCompletionSource<object>();

      using (BsonWriter bsonWriter = new BsonWriter(writeStream) { CloseOutput = false })
      {
        JsonSerializer jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings);
        jsonSerializer.Serialize(bsonWriter, value);
        bsonWriter.Flush();
        tcs.SetResult(null);
      }

      return tcs.Task;
    }
  }
}
