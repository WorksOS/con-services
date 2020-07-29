using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for Designs resource.
  /// </summary>
  [Route("api/v2/designs")]
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class DesignController : ProductionDataBaseController<DesignController>, IDesignContract
  {
    /// <summary>
    /// Gets a list of design boundaries in GeoJson format from Raptor.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <param name="tolerance">The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.</param>
    [ProjectVerifier]
    [HttpGet("boundaries")]
    public async Task<ContractExecutionResult> GetDesignBoundaries([FromQuery] Guid projectUid, [FromQuery] double? tolerance)
    {
      Log.LogInformation($"{nameof(GetDesignBoundaries)}: " + Request.QueryString);

      var projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      tolerance ??= DesignBoundariesRequest.BOUNDARY_POINTS_INTERVAL;

      var request = new DesignBoundariesRequest(projectId, projectUid, tolerance.Value);

      request.Validate();

      var fileList = await FileImportProxy.GetFiles(projectUid.ToString(), GetUserId(), Request.Headers.GetCustomHeaders());

      fileList = fileList?.Where(f => f.ImportedFileType == ImportedFileType.DesignSurface && f.IsActivated).ToList();

      return await RequestExecutorContainerFactory.Build<DesignExecutor>(
        LoggerFactory,
        configStore: ConfigStore,
        fileList: fileList,
        trexCompactionDataProxy: TRexCompactionDataProxy)
        .ProcessAsync(request);
    }

    /// <summary>
    /// Gets a list of models describing the geometry and station labeling for the master alignment in an alignment design.
    /// Arcs may be optionally converted to poly lines with a specified arc chord tolerance.
    /// </summary>
    [ProjectVerifier]
    [HttpGet("alignment/master/geometries")]
    public async Task<IActionResult> GetAlignmentGeometriesForRendering(
      [FromQuery] Guid projectUid,
      [FromQuery] bool convertArcsToChords,
      [FromQuery] double arcChordTolerance)
    {
      Log.LogInformation($"{nameof(GetAlignmentGeometriesForRendering)}: " + Request.QueryString);

      var fileList = await FileImportProxy.GetFiles(projectUid.ToString(), GetUserId(), Request.Headers.GetCustomHeaders());

      fileList = fileList?.Where(f => f.ImportedFileType == ImportedFileType.Alignment && f.IsActivated).ToList();

      if (fileList.Count > 0)
      {
        var alignmentGeometries = new List<AlignmentGeometry>();

        var tasks = new List<Task<ContractExecutionResult>>();

        foreach (var file in fileList)
        {
          if (Guid.TryParse(file.ImportedFileUid, out var designUid))
          {
            Log.LogInformation($"Processing alignment data. File UID: {designUid}, File Name: {file.Name}");

            var request = new AlignmentGeometryRequest(projectUid, designUid, convertArcsToChords, arcChordTolerance, file.Name);
            request.Validate();

            var result = RequestExecutorContainerFactory.Build<AlignmentGeometryExecutor>(LoggerFactory,
              configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy).ProcessAsync(request);

            tasks.Add(result);
          }
          else
            Log.LogInformation($"Invalid alignment data file UID: {designUid}. File Name: {file.Name}");
        }

        Task.WaitAll(tasks.ToArray());

        foreach (var task in tasks)
          alignmentGeometries.Add((task.Result as AlignmentGeometryResult).AlignmentGeometry);

        return StatusCode((int)HttpStatusCode.OK, alignmentGeometries.ToArray());
      }

      Log.LogInformation($"Project {projectUid} does not have any alignment data.");

      return NoContent();
    }

    /// <summary>
    /// Gets a model describing the geometry and station labeling for the master alignment in an alignment design
    /// Arcs may be optionally converted to poly lines with a specified arc chord tolerance
    /// </summary>
    [ProjectVerifier]
    [HttpGet("alignment/master/geometry")]
    public async Task<ContractExecutionResult> GetAlignmentGeometryForRendering(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid designUid,
      [FromQuery] string fileName,
      [FromQuery] bool convertArcsToChords,
      [FromQuery] double arcChordTolerance)
    {
      Log.LogInformation($"{nameof(GetAlignmentGeometryForRendering)}: " + Request.QueryString);

      var request = new AlignmentGeometryRequest(projectUid, designUid, convertArcsToChords, arcChordTolerance, fileName);

      request.Validate();

      return await RequestExecutorContainerFactory.Build<AlignmentGeometryExecutor>(LoggerFactory,
        configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy).ProcessAsync(request);
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    private string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }
  }
}
