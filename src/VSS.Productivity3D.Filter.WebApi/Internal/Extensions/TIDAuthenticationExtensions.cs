using Microsoft.AspNetCore.Builder;
using VSS.Productivity3D.Filter.Common.Filters.Authentication;

namespace VSS.Productivity3D.Filter.WebAPI.Internal.Extensions
{
  /// <summary>
  /// Custom extensions for the <see cref="TIDAuthentication"/> class.
  /// </summary>
  public static class TIDAuthenticationExtensions
  {
    /// <summary>
    /// Uses the tid authentication.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseTIDAuthentication(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<TIDAuthentication>();
    }
  }
}