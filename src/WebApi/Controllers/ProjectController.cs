using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VVSS.TagFileAuth.Service.Controllers
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
    /// Gets the project id for the project whose boundary the specified asset is inside at the given location and date time. 
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
      log.LogInformation("GetProjectId: {0}", Request.QueryString);

      request.Validate();
      var result = RequestExecutorContainer.Build<ProjectIdExecutor>(factory, log).Process(request) as GetProjectIdResult;

      if (result.result)
      {
        var infoMessage = string.Format("project/getid was successfully processed. Request {0} Result {1}", JsonConvert.SerializeObject(Request.QueryString), JsonConvert.SerializeObject(result));
        log.LogInformation(infoMessage);
      }
      else
      {
        var errorMessage = string.Format("project/getid failed to be processed. Request {0} Result {1}", JsonConvert.SerializeObject(Request.QueryString), JsonConvert.SerializeObject(result)); ;
        log.LogError(errorMessage);
      }

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
      log.LogInformation("PostProjectBoundary: {0}", Request.QueryString);

      request.Validate();
      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory, log).Process(request) as GetProjectBoundaryAtDateResult;

      if (result.result)
      {
        var infoMessage = string.Format("Project boundary was received successfully. Tag file data/time: {0}", request.tagFileUTC);
        log.LogInformation(infoMessage);
      }
      else
      {
        var errorMessage = string.Format("No Project boundary was received. Tag file data/time: {0}", request.tagFileUTC);
        log.LogError(errorMessage);
      }

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
      log.LogInformation("PostProjectBoundaries: {0}", Request.QueryString);

      request.Validate();
      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, log).Process(request) as GetProjectBoundariesAtDateResult;

      if (result.result)
      {
        var infoMessage = string.Format("Project boundaries were received successfully. Asset ID: {0}, tag file data/time: {1}", request.tagFileUTC);
        log.LogInformation(infoMessage);
      }
      else
      {
        var errorMessage = string.Format("No Project boundaries was received. Asset ID: {0}, tag file data/time: {1}", request.tagFileUTC);
        log.LogError(errorMessage);
      }

      return result;
    }
  }
}