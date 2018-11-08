using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Trex.HTTPClients.Constants;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi
{
  /// <summary>
  /// This is a named HttpClient which is used to send connected site messages
  /// to the connected site backend.
  /// </summary>
  public class ConnectedSiteClient
  {

    //private ILogger<ValuesClient> _logger;
    private HttpClient _client;

    public ConnectedSiteClient(HttpClient client)
    {
      _client = client;
      //_logger = logger;
    }
    
    public async Task<HttpResponseMessage> PostMessage(IConnectedSiteMessage message)
    {   
      try
      {
        StringContent requestContent =  new StringContent(JsonConvert.SerializeObject(message));
        requestContent.Headers.ContentType.MediaType = MediaTypes.JSON;
        return await _client.PostAsync(message.Route, requestContent);
      }
      catch (HttpRequestException ex)
      {
        var msg = ex.ToString();
        //_logger.LogError($"An error occured connecting to ConnectedSite API {ex.ToString()}");
        throw;
      }
    }
  }
}
