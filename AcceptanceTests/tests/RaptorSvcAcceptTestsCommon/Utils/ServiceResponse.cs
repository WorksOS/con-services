using System.Net;

namespace RaptorSvcAcceptTestsCommon.Utils
{
  /// <summary>
  /// Represent an HTTP request requesponse
  /// </summary>
  public class ServiceResponse
  {
    public WebHeaderCollection ResponseHeader { get; set; }
    public HttpStatusCode HttpCode { get; set; }
    public string ResponseBody { get; set; }
  }
}
