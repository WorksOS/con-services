using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Masterdata;
using VSS.TagFileAuth.Service.Executors;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Interfaces;

namespace VVSS.TagFileAuth.Service.Controllers
{
  public class ProjectController : Controller
  {

    /// <summary>
    /// Repository factory for use by executor
    /// </summary>
    private readonly IRepositoryFactory factory;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger logger;


    /// <summary>
    /// Constructor with injected repository factory and logger
    /// </summary>
    /// <param name="factory">Repository factory</param>
    /// <param name="logger">Logger</param>
    public ProjectController(IRepositoryFactory factory, ILogger<ProjectController> logger)
    {
      this.factory = factory;
      this.logger = logger;
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
      public GetProjectIdResult PostProjectId([FromBody]GetProjectIdRequest request)
      {
        request.Validate();
        var result = RequestExecutorContainer.Build<ProjectIdExecutor>(factory).Process(request) as GetProjectIdResult;

        if (result.result)
        {
          var infoMessage = string.Format("Valid Project ID was received successfully. Asset ID: {0}", request.assetId);
          logger.LogInformation(infoMessage);
        }
        else
        {
          var errorMessage = string.Format("Valid Project ID failed to be received. Asset ID: {0}", request.assetId);
          logger.LogError(errorMessage);
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
        request.Validate();
        var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory).Process(request) as GetProjectBoundaryAtDateResult;

        if (result.result)
        {
          var infoMessage = string.Format("Project boundary was received successfully. Tag file data/time: {0}", request.tagFileUTC);
          logger.LogInformation(infoMessage);
        }
        else
        {
          var errorMessage = string.Format("No Project boundary was received. Tag file data/time: {0}", request.tagFileUTC);
          logger.LogError(errorMessage);
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
        request.Validate();
        var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory).Process(request) as GetProjectBoundariesAtDateResult;

      if (result.result)
        {
          var infoMessage = string.Format("Project boundaries were received successfully. Asset ID: {0}, tag file data/time: {1}", request.tagFileUTC);
          logger.LogInformation(infoMessage);
        }
        else
        {
          var errorMessage = string.Format("No Project boundaries was received. Asset ID: {0}, tag file data/time: {1}", request.tagFileUTC);
          logger.LogError(errorMessage);
        }

      return result;
      }
    }
  }