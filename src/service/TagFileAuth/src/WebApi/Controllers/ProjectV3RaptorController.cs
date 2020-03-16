using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Project controller for C2S2 solution, to support Raptor.
  /// </summary>
  public class ProjectV3RaptorController : BaseController
  {
    private readonly ILogger log;

    /// <summary>
    /// Default constructor 
    /// </summary>
    public ProjectV3RaptorController(ILoggerFactory logger, IConfigurationStore configStore,
      IProjectProxy projectProxy, ICustomerProxy customerProxy, IDeviceProxy deviceProxy)
      : base(logger, configStore, projectProxy, customerProxy, deviceProxy)
    {
      log = logger.CreateLogger<ProjectV3RaptorController>();
    }

    /// <summary>
    /// Gets the shortRaptorProjectId for the project whose boundary the location is inside at the given date time. 
    ///    authority is determined by deviceLicenses for the account
    /// </summary>
    /// <param name="request">Details of the device, location and date time</param>
    /// <returns>
    /// The shortRaptorProjectId if the device is inside a project otherwise -1.
    /// </returns>
    [Route("api/v3/project/getId")]
    [HttpPost]
    public async Task<GetProjectIdResult> GetProjectId([FromBody]GetProjectIdRequest request)
    {
      log.LogDebug($"{nameof(GetProjectId)}: request: {JsonConvert.SerializeObject(request)}");
      request.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(log, configStore, projectProxy, customerProxy, deviceProxy);
      var result = await executor.ProcessAsync(request) as GetProjectIdResult;

      log.LogResult(nameof(GetProjectId), request, result);      
      return result;
    }

    /// <summary>
    /// Gets the project boundary for the specified project if it is active at the specified date time. 
    /// </summary>
    /// <param name="request">Details of the project and date time</param>
    /// <returns>
    /// The project boundary as a list of WGS84 lat/lng points in radians.
    /// </returns>
    [Route("api/v3/project/getBoundary")]
    [HttpPost]
    public async Task<GetProjectBoundaryAtDateResult> PostProjectBoundary([FromBody]GetProjectBoundaryAtDateRequest request)
    {
      log.LogDebug($"{nameof(PostProjectBoundary)}: request: {JsonConvert.SerializeObject(request)}");
      request.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(log, configStore, projectProxy, customerProxy, deviceProxy);
      var result = await executor.ProcessAsync(request) as GetProjectBoundaryAtDateResult;

      log.LogResult(nameof(PostProjectBoundary), request, result);
      return result;
    }

    /// <summary>
    /// Gets a list of project boundaries for the owner of the specified device which are active at the specified date time. 
    /// </summary>
    /// <param name="request">Details of the device and date time</param>
    /// <returns>
    /// A list of  project boundaries, each boundary is a list of WGS84 lat/lng points in radians.
    /// </returns>
    [Route("api/v3/project/getBoundaries")]
    [HttpPost]
    public async Task<GetProjectBoundariesAtDateResult> PostProjectBoundaries([FromBody]GetProjectBoundariesAtDateRequest request)
    {
      log.LogDebug($"{nameof(PostProjectBoundaries)}: {JsonConvert.SerializeObject(request)}");
      request.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(log, configStore, projectProxy, customerProxy, deviceProxy);
      var result = await executor.ProcessAsync(request) as GetProjectBoundariesAtDateResult;

      log.LogResult(nameof(PostProjectBoundaries), request, result);
      return result;
    }
  }
}
