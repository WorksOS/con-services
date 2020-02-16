using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
  public class ServiceResponseMessage
  {
    public HttpContent Content { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public MediaTypeHeaderValue ContentType { get; set; }
  }
}
