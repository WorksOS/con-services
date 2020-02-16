using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using log4net;
using Microsoft.ServiceModel.Web;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHBssSvc
{
  /// <summary>
  /// Used to intercept incoming payloads to the BSS Service, for diagnostic purposes.
  /// </summary>
  public class XMLInterceptor : RequestInterceptor
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    public XMLInterceptor() : base(true) { }

    /// <summary>
    /// Logs the received payload to log4net file, to aid diagnostic debugging on the Bss Service.
    /// </summary>
    public override void ProcessRequest(ref RequestContext requestContext)
    {
      if ((requestContext == null || requestContext.RequestMessage == null) && WebOperationContext.Current != null)
      {
        log.IfWarnFormat("Request context is null.");
        WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        return;
      }

      if (requestContext != null && requestContext.RequestMessage != null)
      {
        RemoteEndpointMessageProperty endpoint = requestContext.RequestMessage.Properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
        string ep = string.Empty;
        if (endpoint != null)
          ep = string.Format("{0}:{1}", endpoint.Address, endpoint.Port);

        HttpRequestMessageProperty user = requestContext.RequestMessage.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
        string from = string.Empty;
        if (user != null)
          @from = user.Headers["User-Agent"];

        if (endpoint != null || user != null)
          log.IfInfoFormat("Request from: {0} IP: {1}", string.IsNullOrEmpty(@from) ? "Unknown" : @from, string.IsNullOrEmpty(ep) ? "Unknown" : ep);

        log.IfInfoFormat("Payload: {0}", requestContext.RequestMessage.ToString());
      }
    }
  }
}
