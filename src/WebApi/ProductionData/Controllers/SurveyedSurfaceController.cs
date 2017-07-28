using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
    /// <summary>
    /// Controller for Surveyed Surfaces resource.
    /// </summary>
    /// 
    [ResponseCache(NoStore = true)]
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
    /// Constructor with injection
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
    [PostRequestVerifier]
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectWritableVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [ProjectWritableWithUIDVerifier]
    [Route("api/v1/surveyedsurfaces")]
    [HttpPost]

    public ContractExecutionResult Post([FromBody] SurveyedSurfaceRequest request)
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
    [HttpGet]

    [Route("api/v1/projects/{projectId}/surveyedsurfaces/{surveyedsurfaceId}/delete")]
    public ContractExecutionResult GetDel([FromRoute] long projectId, [FromRoute] long surveyedSurfaceId)
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
    [HttpGet]

    [Route("api/v2/projects/{projectUid}/surveyedsurfaces/{surveyedsurfaceId}/delete")]
    public ContractExecutionResult GetDel([FromRoute] Guid projectUid, [FromRoute] long surveyedSurfaceId)
    {
      long projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      ProjectID projId = ProjectID.CreateProjectID(projectId, projectUid);
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
    [HttpGet]

    [Route("api/v1/projects/{projectId}/surveyedsurfaces")]
    public SurveyedSurfaceResult Get([FromRoute] long projectId)
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
    [HttpGet]

    [Route("api/v2/projects/{projectUid}/surveyedsurfaces")]
    public SurveyedSurfaceResult Get([FromRoute] Guid projectUid)
    {
      long projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      ProjectID request = ProjectID.CreateProjectID(projectId, projectUid);

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
    [PostRequestVerifier]
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectWritableVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [ProjectWritableWithUIDVerifier]
    [Route("api/v1/surveyedsurfaces/post")]
    [HttpPost]
    public ContractExecutionResult PostPut([FromBody] SurveyedSurfaceRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<SurveyedSurfaceExecutorPut>(logger, raptorClient, null).Process(request);
    }

    /// <summary>
    /// Removes specified Design File from DesignProfiler cache.
    /// </summary>
    /// <param name="request">Descriptor of the Design File (filename).</param>
    /// <returns>Execution result.</returns>
    /// 
    [PostRequestVerifier]
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectWritableVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [ProjectWritableWithUIDVerifier]
    [Route("api/v1/designcache/delete")]
    [HttpPost]
    public ContractExecutionResult PostDelete([FromBody] DesignNameRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<DesignNameUpdateCacheExecutor>(logger, raptorClient, null).Process(request);
    }


  }
}
