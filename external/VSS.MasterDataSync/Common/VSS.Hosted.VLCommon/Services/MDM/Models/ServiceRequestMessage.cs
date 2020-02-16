using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
  public class ServiceRequestMessage
  {
    public string RequestPayload { get; set; }
    public Uri RequestUrl { get; set; }
    public HttpMethod RequestMethod { get; set; }
    public string RequestContentType { get; set; }
    public Encoding RequestEncoding { get; set; }
    public List<KeyValuePair<string, string>> RequestHeaders { get; set; }
  }
}
