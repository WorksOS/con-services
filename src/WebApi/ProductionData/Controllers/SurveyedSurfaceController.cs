using System;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.Contracts;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;


namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for Surveyed Surfaces resource.
  /// </summary>
  /// 
  public class SurveyedSurfaceController : Controller, ISurveyedSurfaceContract
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injected raptor client and logger
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    public SurveyedSurfaceController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<SurveyedSurfaceController>();
    }


    /// <summary>
    /// Posts a Surveyed Surface to Raptor.
    /// </summary>
    /// <param name="request">Description of the Surveyed Surface request.</param>
    /// <returns>Execution result.</returns>
    /// 
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectWritableVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [ProjectWritableWithUIDVerifier]
    [System.Web.Http.Route("api/v1/surveyedsurfaces")]
    [System.Web.Http.HttpPost]

    public ContractExecutionResult Post([System.Web.Http.FromBody] SurveyedSurfaceRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<SurveyedSurfaceExecutorPost>(logger, raptorClient, null).Process(request);
    }

    /// <summary>
    /// Deletes a Surveyed Surface form Raptor's list of surveyed surfaces.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <param name="surveyedSurfaceId">The Surveyed Surface identifier.</param>
    /// <returns></returns>
    /// 
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectWritableVerifier]
    [System.Web.Http.HttpGet]

    [System.Web.Http.Route("api/v1/projects/{projectId}/surveyedsurfaces/{surveyedsurfaceId}/delete")]
    public ContractExecutionResult GetDel([FromUri] long projectId, [FromUri] long surveyedSurfaceId)
    {
      ProjectID projId = ProjectID.CreateProjectID(projectId);
      projId.Validate();

      DataID ssId = DataID.CreateDataID(surveyedSurfaceId);
      ssId.Validate();

      return
          RequestExecutorContainer.Build<SurveyedSurfaceExecutorDelete>(logger, raptorClient, null)
              .Process(new Tuple<ProjectID, DataID>(projId, ssId));
    }

    /// <summary>
    /// Deletes a Surveyed Surface form Raptor's list of surveyed surfaces.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <param name="surveyedSurfaceId">The Surveyed Surface identifier.</param>
    /// <returns></returns>
    /// 
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [ProjectWritableWithUIDVerifier]
    [System.Web.Http.HttpGet]

    [System.Web.Http.Route("api/v2/projects/{projectUid}/surveyedsurfaces/{surveyedsurfaceId}/delete")]
    public ContractExecutionResult GetDel([FromUri] Guid projectUid, [FromUri] long surveyedSurfaceId)
    {
      ProjectID projId = ProjectID.CreateProjectID(0, projectUid);
      projId.Validate();

      DataID ssId = DataID.CreateDataID(surveyedSurfaceId);
      ssId.Validate();

      return
          RequestExecutorContainer.Build<SurveyedSurfaceExecutorDelete>(logger, raptorClient, null)
              .Process(new Tuple<ProjectID, DataID>(projId, ssId));
    }

    /// <summary>
    /// Gets a Surveyed Surface list from Raptor.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <returns>Execution result with a list of Surveyed Surfaces.</returns>
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [System.Web.Http.HttpGet]

    [System.Web.Http.Route("api/v1/projects/{projectId}/surveyedsurfaces")]
    public SurveyedSurfaceResult Get([FromUri] long projectId)
    {
      ProjectID request = ProjectID.CreateProjectID(projectId);

      request.Validate();
      return RequestExecutorContainer.Build<SurveyedSurfaceExecutorGet>(logger, raptorClient, null).Process(request) as SurveyedSurfaceResult;
    }

    /// <summary>
    /// Gets a Surveyed Surface list from Raptor.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <returns>Execution result with a list of Surveyed Surfaces.</returns>
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [System.Web.Http.HttpGet]

    [System.Web.Http.Route("api/v2/projects/{projectUid}/surveyedsurfaces")]
    public SurveyedSurfaceResult Get([FromUri] Guid projectUid)
    {
      ProjectID request = ProjectID.CreateProjectID(0, projectUid);

      request.Validate();
      return RequestExecutorContainer.Build<SurveyedSurfaceExecutorGet>(logger, raptorClient, null).Process(request) as SurveyedSurfaceResult;
    }

    /// <summary>
    /// Updates an existing Surveyed Surface data in a Raptor's list of surveyed surfaces if the target
    /// exists, otherwise - adds a new Surveyed Surface to the list.
    /// </summary>
    /// <param name="request">Description of the Surveyed Surface request.</param>
    /// <returns>Execution result.</returns>
    /// 
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectWritableVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [ProjectWritableWithUIDVerifier]
    [System.Web.Http.Route("api/v1/surveyedsurfaces/post")]
    public ContractExecutionResult PostPut([System.Web.Http.FromBody] SurveyedSurfaceRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<SurveyedSurfaceExecutorPut>(logger, raptorClient, null).Process(request);
    }

    /// <summary>
    /// Removes specified Design File from DesignProfiler cache.
    /// </summary>
    /// <param name="request">Descriptor of the Design File (filename).</param>
    /// <returns>Execution result.</returns>
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectWritableVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [ProjectWritableWithUIDVerifier]
    [System.Web.Http.Route("api/v1/designcache/delete")]
    public ContractExecutionResult PostDelete([System.Web.Http.FromBody] DesignNameRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<DesignNameUpdateCacheExecutor>(logger, raptorClient, null).Process(request);
    }


  }
}
