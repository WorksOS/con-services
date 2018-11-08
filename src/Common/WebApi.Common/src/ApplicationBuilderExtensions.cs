using Microsoft.AspNetCore.Builder;

namespace VSS.WebApi.Common
{
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseFilterMiddleware<T>(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<T>();
    }
  }
}
