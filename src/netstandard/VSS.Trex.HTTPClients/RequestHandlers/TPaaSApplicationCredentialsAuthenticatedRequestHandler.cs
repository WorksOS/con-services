using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VSS.Trex.HTTPClients.RequestHandlers
{
  public class TPaaSApplicationCredentialsRequestHandler : DelegatingHandler
  {
    private string TokenType = "Basic";
    private string TPaaSToken  = "R1doOVJad29Wa19lT1dpUFJkSlhGZWk5YmJvYTozMndPT0VoZ2luVDdrSjY0RTVHal9ZSGVqZ1Fh";

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
