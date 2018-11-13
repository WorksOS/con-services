using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VSS.TRrex.HttpClients.Abstractions;
namespace VSS.TRex.HttpClients.RequestHandlers
{
  /// <summary>
  /// This is a request handler which acts a middleware to outingoing requests
  /// which handles all necessary TPaaS authentication for the application.
  /// 
  /// Example usage (in Startup.cs)
  /// 
  ///   ...
  ///    services.AddTransient<TPaaSAuthenticatedRequestHandler>();
  ///    services.AddHttpClient<YOUR_TYPED_HTTP_CLIENT>(client => 
  ///        client.BaseAddress = new Uri("YOUR_BASE_URI")
  ///      )
  ///      .AddHttpMessageHandler<TPaaSAuthenticatedRequestHandler>();
  ///   ...
  ///   
  /// see <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.1">MS Documentation</a>
  /// for full details of the pattern.
  /// </summary>
  public class TPaaSAuthenticatedRequestHandler : DelegatingHandler
  {
    private ITPaaSClient tpassClient;

    public TPaaSAuthenticatedRequestHandler(ITPaaSClient tpassClient) : base()
    {
      this.tpassClient = tpassClient;
    }

    public TPaaSAuthenticatedRequestHandler(HttpMessageHandler innerHandler, ITPaaSClient tpassClient) :
      base(innerHandler)
    {
      this.tpassClient = tpassClient;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      
      string bearerToken = await tpassClient.GetBearerTokenAsync();
      if (string.IsNullOrEmpty(bearerToken))
      {
        throw new TPaaSAuthenticatedRequestHandlerException("Bearer could not be obtained, have you DI'd the TPaaSAppCreds Client?");
      }

      if (!request.Headers.Contains("Authorization"))
      {
        request.Headers.Add("Authorization", bearerToken);
      }

      return await base.SendAsync(request, cancellationToken);
    }
  }
}
