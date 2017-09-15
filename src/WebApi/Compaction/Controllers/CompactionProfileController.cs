using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Compaction.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionProfileController : BaseController
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
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient">The raptor client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configStore">Configuration store</param>/// 
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    public CompactionProfileController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy) : 
      base (logger.CreateLogger<BaseController>(),exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      log = logger.CreateLogger<CompactionProfileController>();
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="profileResultHelper">Helper to convert/calculate some profile results</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startLatDegrees">Start profileLine Lat</param>
    /// <param name="startLonDegrees">Start profileLine Lon</param>
    /// <param name="endLatDegrees">End profileLine Lat</param>
    /// <param name="endLonDegrees">End profileLine Lon</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutfillDesignUid">Design UID</param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations <see cref="ContractExecutionResult"/>
    /// </returns>
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/profiles/productiondata/slicer")]
    [HttpGet]
    public async Task<CompactionProfileResult<CompactionProfileDataResult>> GetProfileProductionDataSlicer(
      [FromServices] ICompactionProfileResultHelper profileResultHelper,
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutfillDesignUid)
    {
      log.LogInformation("GetProfileProductionDataSlicer: " + Request.QueryString);
      var projectId = GetProjectId(projectUid);

      var settings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid, null, null, null, null, null, null, null, null, null);
      var cutFillDesign = await GetDesignDescriptor(projectUid, cutfillDesignUid, true);

      //Get production data profile
      var slicerProductionDataProfileRequest = requestFactory.Create<ProductionDataProfileRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(settings)
          .Filter(filter)
          .DesignDescriptor(cutFillDesign))
          .CreateProductionDataProfileRequest(startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees);

      slicerProductionDataProfileRequest.Validate();

      var slicerProductionDataResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(logger, raptorClient)
          .Process(slicerProductionDataProfileRequest) as CompactionProfileResult<CompactionProfileCell>
      );

      if (cutfillDesignUid.HasValue)
      {
        //Get design profile
        var slicerDesignProfileRequest = requestFactory.Create<DesignProfileRequestHelper>(r => r
            .ProjectId(projectId)
            .Headers(customHeaders)
            .ProjectSettings(settings)
            .DesignDescriptor(cutFillDesign))
            .CreateDesignProfileRequest(startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees);

        slicerDesignProfileRequest.Validate();

        var slicerDesignResult = WithServiceExceptionTryExecute(() =>
          RequestExecutorContainerFactory
            .Build<CompactionDesignProfileExecutor>(logger, raptorClient)
            .Process(slicerDesignProfileRequest) as CompactionProfileResult<CompactionProfileVertex>
        );

        //Find the cut-fill elevations for the cell stations from the design vertex elevations
        profileResultHelper.FindCutFillElevations(slicerProductionDataResult, slicerDesignResult);
      }
      var transformedResult = profileResultHelper.ConvertProfileResult(slicerProductionDataResult);
      profileResultHelper.RemoveRepeatedNoData(transformedResult);
      return transformedResult;
    }

    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/profiles/design/slicer")]
    [HttpGet]
    public async Task<CompactionProfileResult<CompactionDesignProfileResult>> GetProfileDesignSlicer(
      [FromServices] ICompactionProfileResultHelper profileResultHelper,
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] Guid[] importedFileUid,
      [FromQuery] Guid? filterUid = null)
    {
      log.LogInformation("GetProfileDesignSlicer: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);
      var settings = await GetProjectSettings(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid, null, null, null, null, null, null, null, null, null);

      if (importedFileUid.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "At least one importedFileUId must be specified"));
      }

      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> results =
        new Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>();
      foreach (var impFileUid in importedFileUid)
      {
        var designDescriptor = await GetDesignDescriptor(projectUid, impFileUid, true);

        var profileRequest = requestFactory.Create<DesignProfileRequestHelper>(r => r
            .ProjectId(projectId)
            .Headers(customHeaders)
            .ProjectSettings(settings)
            .Filter(filter)
            .DesignDescriptor(designDescriptor))
          .CreateDesignProfileRequest(startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees);

        profileRequest.Validate();
        var slicerDesignResult = WithServiceExceptionTryExecute(() =>
          RequestExecutorContainerFactory
            .Build<CompactionDesignProfileExecutor>(logger, raptorClient)
            .Process(profileRequest) as CompactionProfileResult<CompactionProfileVertex>
        );
        results.Add(impFileUid, slicerDesignResult);
      }

      var transformedResult = profileResultHelper.ConvertProfileResult(results);
      profileResultHelper.AddSlicerEndPoints(transformedResult);
      return transformedResult;
    }
  }
}