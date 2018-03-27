using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionProfileController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient">The raptor client.</param>
    /// <param name="loggerFactory">The loggerFactory.</param>
    /// <param name="configStore">Configuration store</param>/// 
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    public CompactionProfileController(IASNodeClient raptorClient, ILoggerFactory loggerFactory, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy) :
      base(loggerFactory, loggerFactory.CreateLogger<CompactionProfileController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
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
    /// <param name="filterUid">Filter UID for all profiles except summary volumes</param>
    /// <param name="cutfillDesignUid">Design UID for cut-fill</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or  filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeCalcType">Summary volumes calculation type</param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations <see cref="ContractExecutionResult"/>
    /// </returns>
    [ProjectUidVerifier(AllowLandfillProjects = true)]
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
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] Guid? volumeBaseUid,
      [FromQuery] Guid? volumeTopUid,
      [FromQuery] VolumeCalcType? volumeCalcType)
    {
      Log.LogInformation("GetProfileProductionDataSlicer: " + Request.QueryString);
      var projectId = GetLegacyProjectId(projectUid);

      var settings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var cutFillDesign = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid, true);

      FilterResult baseFilter = null;
      FilterResult topFilter = null;
      DesignDescriptor volumeDesign = null;
      if (volumeCalcType.HasValue)
      {
        switch (volumeCalcType.Value)
        {
          case VolumeCalcType.GroundToGround:
            baseFilter = await GetCompactionFilter(projectUid, volumeBaseUid);
            topFilter = await GetCompactionFilter(projectUid, volumeTopUid);
            break;
          case VolumeCalcType.GroundToDesign:
            baseFilter = await GetCompactionFilter(projectUid, volumeBaseUid);
            volumeDesign = await GetAndValidateDesignDescriptor(projectUid, volumeTopUid, true);
            break;
          case VolumeCalcType.DesignToGround:
            volumeDesign = await GetAndValidateDesignDescriptor(projectUid, volumeBaseUid, true);
            topFilter = await GetCompactionFilter(projectUid, volumeTopUid);
            break;
        }
      }

      //Get production data profile
      var slicerProductionDataProfileRequest = requestFactory.Create<ProductionDataProfileRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(settings)
          .Filter(filter)
          .DesignDescriptor(cutFillDesign))
          .SetBaseFilter(baseFilter)
          .SetTopFilter(topFilter)
          .SetVolumeCalcType(volumeCalcType)
          .SetVolumeDesign(volumeDesign)
          .CreateProductionDataProfileRequest(startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees);

      slicerProductionDataProfileRequest.Validate();

      var slicerProductionDataResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(LoggerFactory, raptorClient, null, null, null, null, null, profileResultHelper)
          .Process(slicerProductionDataProfileRequest) as CompactionProfileResult<CompactionProfileDataResult>
      );

      if (cutFillDesign != null)
      {
        FindCutFillElevations(projectId, settings, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees,
          cutFillDesign, profileResultHelper, slicerProductionDataResult, CompactionDataPoint.CUT_FILL, VolumeCalcType.None);
      }

      if (volumeDesign != null && (volumeCalcType == VolumeCalcType.DesignToGround || volumeCalcType == VolumeCalcType.GroundToDesign))
      {
        FindCutFillElevations(projectId, settings, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees,
          volumeDesign, profileResultHelper, slicerProductionDataResult, CompactionDataPoint.SUMMARY_VOLUMES, volumeCalcType.Value);
      }
      return slicerProductionDataResult;
    }

    /// <summary>
    /// Calculate the elevations for cut-fill or summary volumes cells from the design surface.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="settings">Project settings</param>
    /// <param name="startLatDegrees">The start latitude of the slicer line in decimal degrees</param>
    /// <param name="startLonDegrees">The start longitude of the slicer line in decimal degrees</param>
    /// <param name="endLatDegrees">The end latitude of the slicer line in decimal degrees</param>
    /// <param name="endLonDegrees">The end longitude of the slicer line in decimal degrees</param>
    /// <param name="design">The design surface descriptor</param>
    /// <param name="profileResultHelper">Utility class to do the work</param>
    /// <param name="slicerProductionDataResult">The slicer profile results containing the production data profiles</param>
    /// <param name="type">The type of profile, either cut-fill or summary volumes</param>
    /// <param name="volumeCalcType">Summary volumes calculation type</param>
    private void FindCutFillElevations(
      long projectId,
      CompactionProjectSettings settings,
      double startLatDegrees, double startLonDegrees,
      double endLatDegrees, double endLonDegrees,
      DesignDescriptor design,
      ICompactionProfileResultHelper profileResultHelper,
      CompactionProfileResult<CompactionProfileDataResult> slicerProductionDataResult,
      string type,
      VolumeCalcType volumeCalcType)
    {
      //Get design profile
      var slicerDesignProfileRequest = requestFactory.Create<DesignProfileRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(settings)
          .DesignDescriptor(design))
        .CreateDesignProfileRequest(startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees);

      slicerDesignProfileRequest.Validate();

      var slicerDesignResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionDesignProfileExecutor>(LoggerFactory, raptorClient)
          .Process(slicerDesignProfileRequest) as CompactionProfileResult<CompactionProfileVertex>
      );

      //Find the cut-fill elevations for the cell stations from the design vertex elevations
      profileResultHelper.FindCutFillElevations(slicerProductionDataResult, slicerDesignResult, type, volumeCalcType);
    }

    /// <summary>
    /// Resource to get a profile design slicer.
    /// </summary>
    [ProjectUidVerifier(AllowLandfillProjects = true)]
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
      Log.LogInformation("GetProfileDesignSlicer: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var settings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);

      if (importedFileUid.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "At least one importedFileUid must be specified"));
      }

      var results = new Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>();

      foreach (Guid impFileUid in importedFileUid)
      {
        var designDescriptor = await GetAndValidateDesignDescriptor(projectUid, impFileUid, true);

        var profileRequest = requestFactory.Create<DesignProfileRequestHelper>(r => r
            .ProjectId(projectId)
            .Headers(this.CustomHeaders)
            .ProjectSettings(settings)
            .Filter(filter)
            .DesignDescriptor(designDescriptor))
          .CreateDesignProfileRequest(startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees);

        profileRequest.Validate();
        var slicerDesignResult = WithServiceExceptionTryExecute(() =>
          RequestExecutorContainerFactory
            .Build<CompactionDesignProfileExecutor>(LoggerFactory, raptorClient)
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