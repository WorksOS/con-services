using Microsoft.AspNetCore.Http;

namespace VSS.MasterData.Project.WebAPI.Middleware
{
  /// <summary>
  /// Project authentication middleware
  /// </summary>
  public class ProjectAuthentication : TIDAuthentication
  {
    /// <summary>
    /// project specific logic for requiring customerUid
    /// </summary>
    public override bool RequireCustomerUid(HttpContext context)
    {
      return !(context.Request.Path.Value.Contains("api/v3/project") && context.Request.Method != "GET");
    } 
  }
}
