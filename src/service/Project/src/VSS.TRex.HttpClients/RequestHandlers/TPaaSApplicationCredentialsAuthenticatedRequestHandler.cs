using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VSS.TRex.HttpClients.RequestHandlers
{
  public class TPaaSApplicationCredentialsRequestHandler : DelegatingHandler
  {

    private const string TokenType = "Basic"; 

    //Key for the environment value for the base 64 encoded application key
    public const string TPAAS_APP_TOKEN__ENV_KEY = "TPAAS_APP_TOKEN"; 

    public TPaaSApplicationCredentialsRequestHandler() : base() {
    }

    public TPaaSApplicationCredentialsRequestHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }



    /// <summary>
    /// This is the base64 encoded application credentials app in form of key:secret
    /// </summary>
    public string TPaaSToken { get; set; } = string.Empty;

    public void SetTPaasToken(string token)
    {
      TPaaSToken = token;
    }

      

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {

      if (!request.Headers.Contains("Authorization"))
      {
        request.Headers.Add("Authorization", $"{TokenType} {TPaaSToken}");
      }

      return await base.SendAsync(request, cancellationToken);
    }

  }
}
