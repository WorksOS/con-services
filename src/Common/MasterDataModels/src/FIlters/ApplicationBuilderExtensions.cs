using System;
using Microsoft.AspNetCore.Builder;

namespace VSS.MasterData.Models.FIlters
{
  //TODO: Remove when all services use WebApi package
  [Obsolete]
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseFilterMiddleware<T>(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<T>();
    }
  }
}
