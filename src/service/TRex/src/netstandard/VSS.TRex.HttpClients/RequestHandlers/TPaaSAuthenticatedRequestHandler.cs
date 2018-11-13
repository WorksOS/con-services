using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VSS.TRex.DI;
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

    public TPaaSAuthenticatedRequestHandler() : base() { }

    public TPaaSAuthenticatedRequestHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      try
      {
        string bearerToken = await DIContext.Obtain<ITPaaSClient>().GetBearerTokenAsync().ConfigureAwait(false);

        if (!request.Headers.Contains("Authorization"))
        {
          request.Headers.Add("Authorization", bearerToken);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
      }
      catch (ArgumentNullException ex)
      {
        throw new TPaaSAuthenticatedRequestHandlerException("Bearer could not be obtained, have you DI'd the TPaaSAppCreds Client?", ex);
      }
    }
  }
}
