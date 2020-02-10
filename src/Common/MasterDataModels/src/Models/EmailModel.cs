using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  public class EmailModel
  {
    [JsonProperty(PropertyName = "fromName", Required = Required.Always)]
    public string FromName { get; set; }
    
    [JsonProperty(PropertyName = "to", Required = Required.Always)]
    public string[] To { get; set; }

    [JsonProperty(PropertyName = "subject", Required = Required.Always)]
    public string Subject { get; set; }
    
    [JsonProperty(PropertyName = "contentBody", Required = Required.Always)]
    public string ContentBody { get; private set; }

    public void SetContent (string content)
    {
      var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(content);
      ContentBody = System.Convert.ToBase64String(plainTextBytes);
    }

  }
}
