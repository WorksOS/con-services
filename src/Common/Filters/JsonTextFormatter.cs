using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.Common.Filters
{
  public class BinaryImageResponseContainer
  {
    public byte[] payload;
    public string code;
  }

  public class RawFileContainer
  {
    public byte[] fileContents;
  }

  /// <summary>
  /// Custom JSON serializer filter (formatter).
  /// </summary>
  public class JsonTextFormatter : MediaTypeFormatter
  {
#pragma warning disable 1591
    public readonly JsonSerializerSettings JsonSerializerSettings;
    private readonly UTF8Encoding _encoding;
    static Object locker = new object();

    public JsonTextFormatter(JsonSerializerSettings jsonSerializerSettings = null)
    {
      this.JsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();

      SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
      this._encoding = new UTF8Encoding(false, true);
      SupportedEncodings.Add(this._encoding);
    }

    public override bool CanReadType(Type type)
    {
      if (type == null)
      {
        throw new ArgumentNullException();
      }

      return true;
    }

    public override bool CanWriteType(Type type)
    {
      return true;
    }

    public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content,
            IFormatterLogger formatterLogger)
    {
      JsonSerializer serializer = JsonSerializer.Create(this.JsonSerializerSettings);

      return Task.Factory.StartNew(() =>
      {
        using (
                StreamReader streamReader = new StreamReader(readStream,
                        this._encoding))
        {
          using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
          {
            var serializedObj = serializer.Deserialize(jsonTextReader, type);

            if (serializedObj is ProjectID)
            {
              var projectId = serializedObj as ProjectID;

              //if (projectId.projectUid.HasValue)
              //  projectId.projectId = RaptorDb.GetProjectID(projectId.projectUid.ToString());
            }
            return serializedObj;
          }
        }
      }, TaskCreationOptions.AttachedToParent);
    }

    public override Task WriteToStreamAsync(Type type, Object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
    {

      if (value is BinaryImageResponseContainer)
        return Task.Factory.StartNew(() =>
        {
          lock (locker)
          {
            BinaryImageResponseContainer container = value as BinaryImageResponseContainer;
            content.Headers.Clear();
            content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Headers.Add("X-Warning", container.code);
            new BinaryWriter(writeStream).Write((byte[])(container.payload));
          }
        }, TaskCreationOptions.AttachedToParent);
      else if (value is RawFileContainer)
      {
        return Task.Factory.StartNew(() =>
        {
          lock (locker)
          {
            RawFileContainer container = value as RawFileContainer;
            content.Headers.Clear();
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //content.Headers.Add("X-Warning", container.code);
            new BinaryWriter(writeStream).Write((byte[])(container.fileContents));
          }
        }, TaskCreationOptions.AttachedToParent);
      }
      JsonSerializer serializer = JsonSerializer.Create(this.JsonSerializerSettings);
      return Task.Factory.StartNew(() =>
      {
        lock (locker)
        {
          using (
              JsonTextWriter jsonTextWriter =
                  new JsonTextWriter(new StreamWriter(writeStream,
                      this._encoding))
                  {
                    CloseOutput = false
                  })
          {
            serializer.Serialize(jsonTextWriter, value);
            jsonTextWriter.Flush();
          }
        }
      }, TaskCreationOptions.AttachedToParent);
    }
#pragma warning restore 1591
  }
}
