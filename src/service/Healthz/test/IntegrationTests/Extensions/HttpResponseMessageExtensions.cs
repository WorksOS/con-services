using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.Types;

namespace IntegrationTests.Extensions
{
  internal static class HttpResponseMessageExtensions
  {
    public static Task<HttpResponseMessage> ValidateResponse(this Task<HttpResponseMessage> response, Validation validation) =>
      validation switch
      {
        Validation.IsSuccess => response.Result.IsSuccessStatusCode ? response : null,
        _ => response
      };
  }
}
