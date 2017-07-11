using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories;
using VSS.Productivity3D.WebApiModels.Executors;
using VSS.Productivity3D.WebApiModels.Models;
using VSS.Productivity3D.WebApiModels.ResultHandling;
using VSS.Productivity3D.WebApiModels.Utilities;

namespace VSS.Productivity3D.WebApi.Controllers
{
  public class ProjectController : Controller
  {
    private readonly IRepositoryFactory factory;
    private readonly ILogger log;

    public ProjectController(IRepositoryFactory factory, ILogger<ProjectController> logger)
    {
      this.factory = factory;
      this.log = logger;
    }

    /// <summary>
    /// Gets the legacyProjectId for the project whose boundary the location is inside at the given date time. 
    ///    authority is determined by servicePlans from the provided legacyAssetId and/or TCCOrgID .
    /// </summary>
    /// <param name="request">Details of the asset, location and date time</param>
    /// <returns>
    /// The project id if the asset is inside a project otherwise -1.
    /// </returns>
    /// <executor>ProjectIdExecutor</executor>
    [Route("api/v1/project/getId")]
    [HttpPost]
    public GetProjectIdResult GetProjectId([FromBody]GetProjectIdRequest request)
    {
      log.LogDebug("GetProjectId: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();

      var result = RequestExecutorContainer.Build<ProjectIdExecutor>(factory, log).Process(request) as GetProjectIdResult;

      log.LogResult(this.ToString(), request, result);      
      return result;
    }

    /// <summary>
    /// Gets the project boundary for the specified project if it is active at the specified date time. 
    /// </summary>
    /// <param name="request">Details of the project and date time</param>
    /// <returns>
    /// The project boundary as a list of WGS84 lat/lng points in radians.
    /// </returns>
    /// <executor>ProjectBoundaryAtDateExecutor</executor>
    [Route("api/v1/project/getBoundary")]
    [HttpPost]
    public GetProjectBoundaryAtDateResult PostProjectBoundary([FromBody]GetProjectBoundaryAtDateRequest request)
    {
      log.LogDebug("PostProjectBoundary: {0}", JsonConvert.SerializeObject(request));
      request.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory, log).Process(request) as GetProjectBoundaryAtDateResult;

      log.LogResult(this.ToString(), request, result);
      return result;
    }

    /// <summary>
    /// Gets a list of project boundaries for the owner of the specified asset which are active at the specified date time. 
    /// </summary>
    /// <param name="request">Details of the asset and date time</param>
    /// <returns>
    /// A list of  project boundaries, each boundary is a list of WGS84 lat/lng points in radians.
    /// </returns>
    /// <executor>ProjectBoundariesAtDateExecutor</executor>
    [Route("api/v1/project/getBoundaries")]
    [HttpPost]
    public GetProjectBoundariesAtDateResult PostProjectBoundaries([FromBody]GetProjectBoundariesAtDateRequest request)
    {
      log.LogDebug("PostProjectBoundaries: {0}", JsonConvert.SerializeObject(request));
      request.Validate();
      
      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, log).Process(request) as GetProjectBoundariesAtDateResult;

      log.LogResult(this.ToString(), request, result);
      return result;
    }
  }
}