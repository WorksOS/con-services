using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.TRex.HttpClients.Constants;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi
{
  /// <summary>
  /// This is a named HttpClient which is used to send connected site messages
  /// to the connected site backend.
  /// </summary>
  public class ConnectedSiteClient : IConnectedSiteClient
  {

    private ILogger<ConnectedSiteClient> _logger;
    private HttpClient _client;

    /// <summary>
    /// Typed HttpClient for sending status messages to connected site.
    /// </summary>
    /// <param name="client">Inner http client</param>
    public ConnectedSiteClient(HttpClient client, ILogger<ConnectedSiteClient> logger)
    {
      _client = client;
      _logger = logger;
    }

    /// <summary>
    /// Post a status message to connected site
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> PostMessage<T>(T message) where T : IConnectedSiteMessage
    {
      StringContent requestContent = new StringContent(JsonConvert.SerializeObject(message));
      requestContent.Headers.ContentType.MediaType = MediaTypes.JSON;
      _logger.LogDebug($"Posting position to connected site {message.Route}");
      return await _client.PostAsync(message.Route, requestContent).ConfigureAwait(false);
    }
  }
}
