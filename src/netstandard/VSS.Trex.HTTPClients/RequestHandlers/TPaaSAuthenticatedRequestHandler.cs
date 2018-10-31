using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VSS.Trex.HTTPClients.Clients;
using VSS.TRex.DI;

namespace VSS.TRex.HTTPClients.RequestHandlers
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
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      //DI.DIContext.Obtain<TPaaSClient>().GetBearerToken();

      if (!request.Headers.Contains("Authorization"))
      {
        request.Headers.Add("Authorization", DIContext.Obtain<TPaaSClient>().GetBearerToken());
      }

      return await base.SendAsync(request, cancellationToken);
    }
  }
}
