using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.Now3D.Controllers
{
  public class ReportController : BaseController
  {
    public ReportController(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    /// <summary>
    /// Get the pass count summary for the project extents
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    [HttpGet("api/v1/reports/passcount")]
    [ProducesResponseType(typeof(PassCountSummaryResult), 200)]
    public IActionResult PasscountReport(string projectUid)
    {
      Log.LogInformation($"PasscountReport Project UID: {projectUid}");
      // Request projectUid, filterUid (optional) 
      // Default filter to project extents, no design
      // URL:  api/v2/passcounts/summary
      // Returns PassCountSummaryResult
      return Json(new PassCountSummaryResult(new TargetPassCountRange(0, 0), 
        true, 
        0, 
        0, 
        0, 
        0, 
        0));
    }

    /// <summary>
    /// Gets the CMV Summary report for the project extents
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    [HttpGet("api/v1/reports/cmv")]
    [ProducesResponseType(typeof(CMVSummaryResult), 200)]
    public IActionResult CmvReport(string projectUid)
    {
      Log.LogInformation($"CmvReport Project UID: {projectUid}");
      // Request: CMVRequest
      // Default filter to project extents, no design
      // URL api/v1/compaction/cmv/summary
      // Response CMVSummaryResult
      return Json(new CMVSummaryResult(0, 
        0, 
        true, 
        0, 
        0, 
        0, 
        0));
    }

    /// <summary>
    /// Gets the MDP Summary report for the project extents
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    [HttpGet("api/v1/reports/mdp")]
    [ProducesResponseType(typeof(CompactionMdpSummaryResult), 200)]
    public IActionResult MdpReport(string projectUid)
    {
      Log.LogInformation($"MdpReport Project UID: {projectUid}");
      // Request: projectUid and filterUid (optional)
      // Url api/v2/mdp/summary
      // Returns CompactionMdpSummaryResult
      return Json(new CompactionMdpSummaryResult(
        new MDPSummaryResult(0, 
          0,
          true, 
          0, 
          0, 
          0, 
          0),
        new MDPSettings(0,
          0,
          0,
          0,
          0,
          true)));
    }

    /// <summary>
    /// Gets a Volumes (Ground to Design) summary report for the project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="designFileUid">The Design File UID to use</param>
    [HttpGet("api/v1/reports/volumes")]
    [ProducesResponseType(typeof(SummaryVolumesResult), 200)]
    public IActionResult VolumesReport(string projectUid, string designFileUid)
    {
      Log.LogInformation($"VolumesReport Project UID: {projectUid}, Design File UID: {designFileUid}");
      // Ground to design only
      // SummaryVolumesRequest

      // Returns SummaryVolumesResult
      return Json(SummaryVolumesResult.Create(BoundingBox3DGrid.CreatBoundingBox3DGrid(0,
          0,
          0,
          0,
          0,
          0), 
        0,
        0,
        0,
        0,
        0));
    }

    /// <summary>
    /// Gets a Cut/Fill report for the given project and design
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="cutfillDesignUid">The Cut File Design File UID to use</param>
    [HttpGet("api/v1/reports/cutfill")]
    [ProducesResponseType(typeof(CompactionCutFillDetailedResult), 200)]
    public IActionResult CutFillReport(string projectUid, string cutfillDesignUid)
    {
      Log.LogInformation($"CutFillReport Project UID: {projectUid}, Design File UID: {cutfillDesignUid}");
      // Request orojectUid, filterUid (optional), cutfillDesignUid
      // Url api/v2/cutfill/details"
      // Returns CompactionCutFillDetailedResult
      return Json(new CompactionCutFillDetailedResult(new[] {0.0d}));
    }
  }
}