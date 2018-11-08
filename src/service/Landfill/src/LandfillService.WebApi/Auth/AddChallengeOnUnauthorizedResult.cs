using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace VSS.VisionLink.Utilization.WebApi.Configuration.Principal
{
  /// <summary>
  /// 
  /// </summary>
  public class AddChallengeOnUnauthorizedResult : IHttpActionResult
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="challenge"></param>
    /// <param name="innerResult"></param>
    public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
    {
      Challenge = challenge;
      InnerResult = innerResult;
    }

    /// <summary>
    /// 
    /// </summary>
    public AuthenticationHeaderValue Challenge { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public IHttpActionResult InnerResult { get; private set; }

    /// <summary>
    /// Creates an <see cref="T:System.Net.Http.HttpResponseMessage"/> asynchronously.
    /// </summary>
    /// <returns>
    /// A task that, when completed, contains the <see cref="T:System.Net.Http.HttpResponseMessage"/>.
    /// </returns>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    {
      var response = await InnerResult.ExecuteAsync(cancellationToken);

      if (response.StatusCode == HttpStatusCode.Unauthorized)
      {
        // Only add one challenge per authentication scheme.
        if (response.Headers.WwwAuthenticate.All(h => h.Scheme != Challenge.Scheme))
        {
          response.Headers.WwwAuthenticate.Add(Challenge);
        }
      }

      return response;
    }
  }
}