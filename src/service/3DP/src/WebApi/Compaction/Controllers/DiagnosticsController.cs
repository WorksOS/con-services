using Microsoft.AspNetCore.Mvc;
using System;
using VSS.Productivity3D.Common.Filters.Authentication;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for validating 3D project settings
  /// </summary>
  [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
  public class DiagnosticsController : Controller
  {
  /// <summary>
    /// Default constructor.
    /// </summary>
    public DiagnosticsController() { }
    
    /// <summary>
    /// Invalidates the response cache objects for a given project.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/diagnostics/clearresponsecache")]
    [HttpPut]
    public ActionResult InvalidateCache(Guid projectUid)
    {
      return Ok();
    }
  }
}
