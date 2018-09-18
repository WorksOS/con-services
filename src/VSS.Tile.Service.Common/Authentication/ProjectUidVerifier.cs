using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VSS.Tile.Service.Common.Authentication
{
  /// <summary>
  /// Validation filter attribute for the ProjectUid.
  /// </summary>
  public class ProjectUidVerifier : ActionFilterAttribute
  {
    private const string NAME = "projectUid";

    /// <summary>
    /// Gets or sets whether the Filter will check for and reject Landfill Projects.
    /// </summary>
    public bool AllowLandfillProjects { get; set; }

    /// <summary>
    /// Gets or sets whether the Filter will check for and reject archived Projects.
    /// </summary>
    public bool AllowArchivedState { get; set; }

    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      object projectUidValue = null;

      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];

        // Ignore any query parameter called 'request'.
        if (request.GetType() != typeof(string))
        {
          projectUidValue = request.GetType()
                                   .GetProperty(NAME, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                                   ?.GetValue(request);
        }
      }

      if (actionContext.ActionArguments.ContainsKey(NAME))
      {
        projectUidValue = actionContext.ActionArguments[NAME];
      }

      if (!(projectUidValue is string))
      {
        return;
      }
    }
  }
}
