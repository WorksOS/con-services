using Microsoft.AspNetCore.Builder;

namespace VSS.MasterData.Models.FIlters
{
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseFilterMiddleware<T>(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<T>();
    }
  }
}
