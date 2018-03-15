using Microsoft.AspNetCore.Builder;

namespace VSS.MasterData.Project.WebAPI.Common.Extensions
{
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseFilterMiddleware<T>(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<T>();
    }
  }
}
