using Microsoft.AspNetCore.Builder;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// 
  /// </summary>
  public static class TIDAuthenticationExtensions
  {
    /// <summary>
    /// Uses the tid authentication.
    /// </summary>
    public static IApplicationBuilder UseTIDAuthentication(this IApplicationBuilder builder) =>
      builder.UseMiddleware<TIDAuthentication>();
  }
}
