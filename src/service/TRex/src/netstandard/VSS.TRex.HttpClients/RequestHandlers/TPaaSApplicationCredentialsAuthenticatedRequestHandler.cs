using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VSS.TRex.HttpClients.RequestHandlers
{
  public class TPaaSApplicationCredentialsRequestHandler : DelegatingHandler
  {
    // Key for the environment value for the base 64 encoded application key.
    public const string TPAAS_APP_TOKEN_ENV_KEY = "TPAAS_APP_TOKEN"; 
    private const string TOKEN_TYPE = "Basic"; 

    // This is the base64 encoded application credentials app in form of key:secret.
    public string TPaaSToken { get; set; }

    public TPaaSApplicationCredentialsRequestHandler()
    { }

    public TPaaSApplicationCredentialsRequestHandler(HttpMessageHandler innerHandler)
      : base(innerHandler)
    { }
    
    public void SetTPaasToken(string token)
    {
      TPaaSToken = token;
    }      

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      if (!request.Headers.Contains("Authorization"))
      {
        request.Headers.Add("Authorization", $"{TOKEN_TYPE} {TPaaSToken}");
      }

      return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
  }
}
