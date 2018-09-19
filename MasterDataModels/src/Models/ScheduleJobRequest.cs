using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Used to request an export job to be scheduled
  /// </summary>
  public class ScheduleJobRequest
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public ScheduleJobRequest()
    {
      Headers = new Dictionary<string, string>();
    }

    /// <summary>
    /// The URL to call to get the export data
    /// </summary>
    [JsonProperty(PropertyName = "url", Required = Required.Always)]
    public string Url { get; set; }

    /// <summary>
    /// Custom Headers to be used in the Scheduled job request
    /// </summary>
    [JsonProperty(PropertyName = "headers", Required = Required.Default)]
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>
    /// THe Http method to use. Default is GET.
    /// </summary>
    [JsonProperty(PropertyName = "method", Required = Required.Default)]
    public string Method { get; set; }

    /// <summary>
    /// Payload for POST requests (Body content)
    /// Set this via either SetBinaryPayload or SetStringPayload to ensure encoding is correct
    /// </summary>
    [JsonProperty(PropertyName = "payload", Required = Required.Default)]
    public string Payload { get; private set; }

    /// <summary>
    /// Binary data extracted from the payload, if the payload is binary data. Otherwise null
    /// </summary>
    [JsonIgnore]
    public byte[] PayloadBytes => IsBinaryData ? Convert.FromBase64String(Payload) : null;

    /// <summary>
    /// Is the payload binary data encoded
    /// </summary>
    [JsonProperty(PropertyName = "isBinaryData", Required = Required.Default)]
    public bool IsBinaryData { get; private set; }

    /// <summary>
    /// File name to save export data to
    /// </summary>
    [JsonProperty(PropertyName = "filename", Required = Required.Always)]
    public string Filename { get; set; }

    /// <summary>
    /// Optional timeout for running the scheduled job in milliseconds
    /// </summary>
    [JsonProperty(PropertyName = "timeout", Required = Required.Default)]
    public int? Timeout { get; set; }
    
    /// <summary>
    /// Set the Payload to be binary data from a stream
    /// </summary>
    /// <param name="data">Stream to binary data</param>
    public void SetBinaryPayload(Stream data)
    {
      byte[] bytes;
      if (data is MemoryStream memoryStream)
      {
        bytes = memoryStream.ToArray();
      }
      else
      {
        using (var ms = new MemoryStream())
        {
          data.CopyTo(ms);
          bytes = ms.ToArray();
        }
      }

      IsBinaryData = true;
      Payload = Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Set the payload to be text only.
    /// </summary>
    /// <param name="data">Payload text</param>
    public void SetStringPayload(string data)
    {
      IsBinaryData = false;
      Payload = data;
    }
  }
}
