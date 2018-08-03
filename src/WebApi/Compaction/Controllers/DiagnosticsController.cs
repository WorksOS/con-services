using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using System;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for validating 3D project settings
  /// </summary>
  [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
  public class DiagnosticsController : Controller
  {
    private readonly IResponseCache cache;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DiagnosticsController(IResponseCache cache)
    {
      this.cache = cache;
    }
    
    /// <summary>
    /// Invalidates the response cache objects for a given project.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/diagnostics/clearresponsecache")]
    [HttpPut]
    public ActionResult InvalidateCache(Guid projectUid)
    {
      cache.InvalidateReponseCacheForProject(projectUid);

      return Ok();
    }
  }
}
