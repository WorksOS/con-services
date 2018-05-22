using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for Surveyed Surfaces resource.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class SurveyedSurfaceController : Controller, ISurveyedSurfaceContract
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">LoggerFactory</param>
    public SurveyedSurfaceController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    /// <summary>
    /// Posts a Surveyed Surface to Raptor.
    /// </summary>
    /// <param name="request">Description of the Surveyed Surface request.</param>
    /// <returns>Execution result.</returns>
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [Route("api/v1/surveyedsurfaces")]
    [HttpPost]
    public ContractExecutionResult Post([FromBody] SurveyedSurfaceRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<SurveyedSurfaceExecutorPost>(logger, raptorClient).Process(request);
    }

    /// <summary>
    /// Deletes a Surveyed Surface form Raptor's list of surveyed surfaces.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <param name="surveyedSurfaceId">The Surveyed Surface identifier.</param>
    [Obsolete("This action method is obsolete and no longer used by 3DP application services. It may be in use by Trimble Business Center.")]
    [ProjectIdVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [HttpGet]
    [Route("api/v1/projects/{projectId}/surveyedsurfaces/{surveyedsurfaceId}/delete")]
    public ContractExecutionResult GetDel([FromRoute] long projectId, [FromRoute] long surveyedSurfaceId)
    {
      ProjectID projId = ProjectID.Create(projectId);
      projId.Validate();

      DataID ssId = DataID.CreateDataID(surveyedSurfaceId);
      ssId.Validate();

      return
          RequestExecutorContainerFactory.Build<SurveyedSurfaceExecutorDelete>(logger, raptorClient)
              .Process(new Tuple<ProjectID, DataID>(projId, ssId));
    }

    /// <summary>
    /// Deletes a Surveyed Surface form Raptor's list of surveyed surfaces.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <param name="surveyedSurfaceId">The Surveyed Surface identifier.</param>
    [Obsolete("This action method is obsolete and no longer used by 3DP application services. It may be in use by Trimble Business Center.")]
    [ProjectUidVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [HttpGet]
    [Route("api/v2/projects/{projectUid}/surveyedsurfaces/{surveyedsurfaceId}/delete")]
    public async Task<ContractExecutionResult> GetDel([FromRoute] Guid projectUid, [FromRoute] long surveyedSurfaceId)
    {
      long projectId = await (User as RaptorPrincipal).GetLegacyProjectId(projectUid);
      ProjectID projId = ProjectID.Create(projectId, projectUid);
      projId.Validate();

      DataID ssId = DataID.CreateDataID(surveyedSurfaceId);
      ssId.Validate();

      return
          RequestExecutorContainerFactory.Build<SurveyedSurfaceExecutorDelete>(logger, raptorClient)
              .Process(new Tuple<ProjectID, DataID>(projId, ssId));
    }

    /// <summary>
    /// Gets a Surveyed Surface list from Raptor.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <returns>Execution result with a list of Surveyed Surfaces.</returns>
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [HttpGet]
    [Route("api/v1/projects/{projectId}/surveyedsurfaces")]
    public SurveyedSurfaceResult Get([FromRoute] long projectId)
    {
      ProjectID request = ProjectID.Create(projectId);

      request.Validate();
      return RequestExecutorContainerFactory.Build<SurveyedSurfaceExecutorGet>(logger, raptorClient).Process(request) as SurveyedSurfaceResult;
    }

    /// <summary>
    /// Gets a Surveyed Surface list from Raptor.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <returns>Execution result with a list of Surveyed Surfaces.</returns>
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [HttpGet]
    [Route("api/v2/projects/{projectUid}/surveyedsurfaces")]
    public async Task<SurveyedSurfaceResult> Get([FromRoute] Guid projectUid)
    {
      long projectId = await (User as RaptorPrincipal).GetLegacyProjectId(projectUid);
      ProjectID request = ProjectID.Create(projectId, projectUid);

      request.Validate();
      return RequestExecutorContainerFactory.Build<SurveyedSurfaceExecutorGet>(logger, raptorClient).Process(request) as SurveyedSurfaceResult;
    }

    /// <summary>
    /// Updates an existing Surveyed Surface data in a Raptor's list of surveyed surfaces if the target
    /// exists, otherwise - adds a new Surveyed Surface to the list.
    /// </summary>
    /// <param name="request">Description of the Surveyed Surface request.</param>
    /// <returns>Execution result.</returns>
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [Route("api/v1/surveyedsurfaces/post")]
    [HttpPost]
    public ContractExecutionResult PostPut([FromBody] SurveyedSurfaceRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<SurveyedSurfaceExecutorPut>(logger, raptorClient).Process(request);
    }

    /// <summary>
    /// Removes specified Design File from DesignProfiler cache.
    /// </summary>
    /// <param name="request">Descriptor of the Design File (filename).</param>
    /// <returns>Execution result.</returns>
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [Route("api/v1/designcache/delete")]
    [HttpPost]
    public ContractExecutionResult PostDelete([FromBody] DesignNameRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<DesignNameUpdateCacheExecutor>(logger, raptorClient).Process(request);
    }
  }
}