using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace VSS.VisionLink.Utilization.WebApi.Configuration
{
  /// <summary>
  ///   The result of authentication
  /// </summary>
  public class AuthenticationFailureResult : IHttpActionResult
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reasonPhrase"></param>
    /// <param name="request"></param>
    public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
    {
      ReasonPhrase = reasonPhrase;
      Request = request;
    }

    /// <summary>
    /// 
    /// </summary>
    public string ReasonPhrase { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public HttpRequestMessage Request { get; private set; }

    /// <summary>
    /// Creates an <see cref="T:System.Net.Http.HttpResponseMessage"/> asynchronously.
    /// </summary>
    /// <returns>
    /// A task that, when completed, contains the <see cref="T:System.Net.Http.HttpResponseMessage"/>.
    /// </returns>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    {
      return Task.FromResult(Execute());
    }

    private HttpResponseMessage Execute()
    {
      var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
      {
        RequestMessage = Request,
        ReasonPhrase = ReasonPhrase
      };
      return response;
    }
  }
}