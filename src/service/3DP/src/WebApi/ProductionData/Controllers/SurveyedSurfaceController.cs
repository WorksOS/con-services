using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for Surveyed Surfaces resource.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  [ProjectVerifier]
  public class SurveyedSurfaceController : Controller, ISurveyedSurfaceContract
  {
    private readonly IASNodeClient raptorClient;
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public SurveyedSurfaceController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    /// <summary>
    /// Posts a Surveyed Surface to Raptor.
    /// </summary>
    [PostRequestVerifier]
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
    [Obsolete("This action method is obsolete and no longer used by 3DP application services. It may be in use by Trimble Business Center.")]
    [HttpGet]
    [Route("api/v1/projects/{projectId}/surveyedsurfaces/{surveyedsurfaceId}/delete")]
    public ContractExecutionResult GetDel([FromRoute] long projectId, [FromRoute] long surveyedSurfaceId)
    {
      ProjectID projId = new ProjectID(projectId);
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
    [Obsolete("This action method is obsolete and no longer used by 3DP application services. It may be in use by Trimble Business Center.")]
    [HttpGet]
    [Route("api/v2/projects/{projectUid}/surveyedsurfaces/{surveyedsurfaceId}/delete")]
    public async Task<ContractExecutionResult> GetDel([FromRoute] Guid projectUid, [FromRoute] long surveyedSurfaceId)
    {
      long projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      ProjectID projId = new ProjectID(projectId, projectUid);
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
    [HttpGet]
    [Route("api/v1/projects/{projectId}/surveyedsurfaces")]
    public SurveyedSurfaceResult Get([FromRoute] long projectId)
    {
      ProjectID request = new ProjectID(projectId);

      request.Validate();
      return RequestExecutorContainerFactory.Build<SurveyedSurfaceExecutorGet>(logger, raptorClient).Process(request) as SurveyedSurfaceResult;
    }

    /// <summary>
    /// Gets a Surveyed Surface list from Raptor.
    /// </summary>
    [HttpGet]
    [Route("api/v2/projects/{projectUid}/surveyedsurfaces")]
    public async Task<SurveyedSurfaceResult> Get([FromRoute] Guid projectUid)
    {
      long projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      ProjectID request = new ProjectID(projectId, projectUid);

      request.Validate();
      return RequestExecutorContainerFactory.Build<SurveyedSurfaceExecutorGet>(logger, raptorClient).Process(request) as SurveyedSurfaceResult;
    }

    /// <summary>
    /// Updates an existing Surveyed Surface data in a Raptor's list of surveyed surfaces if the target
    /// exists, otherwise - adds a new Surveyed Surface to the list.
    /// </summary>
    /// <param name="request">Description of the Surveyed Surface request.</param>
    [PostRequestVerifier]
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
    [PostRequestVerifier]
    [Route("api/v1/designcache/delete")]
    [HttpPost]
    public ContractExecutionResult PostDelete([FromBody] DesignNameRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<DesignNameUpdateCacheExecutor>(logger, raptorClient).Process(request);
    }
  }
}
