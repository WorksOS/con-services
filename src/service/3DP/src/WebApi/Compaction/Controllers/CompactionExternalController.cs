using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting production data cell values from TRex
  ///    These are called from external applications using an application context,
  ///     which has no user or customer context.
  ///    They will include the callers applicationName, and may have a device context (DID)
  /// </summary>
  [ProjectVerifier]
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionExternalController : BaseController<CompactionExternalController>
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionExternalController(IConfigurationStore configStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager)
      : base(configStore, fileImportProxy, settingsManager)
    { }


    /// <summary>
    /// Gets a single patch of sub-grids for the project at the machines lat/long. 
    /// </summary>
    /// <remarks>
    /// This endpoint is expected to be used by machine based devices requesting latest elevation and machine times.
    /// The area required is indicated by the bounding box, which is limited to MAX_BOUNDARY_SQUARE_METERSm2.
    /// The response patch of sub-grids is lean and decorated for use with Protobuf-net.
    ///     See GeneratePatchResultProtoFile unit test for generating .proto file for injest by client.
    /// </remarks>
    [HttpGet("api/v2/device/patches")]
    public async Task<IActionResult> GetSubGridPatches(string ecSerial,
      double machineLatitude, double machineLongitude,
      double bottomLeftX, double bottomLeftY, double topRightX, double topRightY)
    {
      var patchesRequest = new PatchesRequest(ecSerial, machineLatitude, machineLongitude,
        new BoundingBox2DGrid(bottomLeftX, bottomLeftY, topRightX, topRightY));
      Log.LogInformation($"{nameof(GetSubGridPatches)}: {JsonConvert.SerializeObject(patchesRequest)}");

      // todoJeannie temporary to look into the DID info available.
      Log.LogDebug($"{nameof(GetSubGridPatches)}: customHeaders {CustomHeaders.LogHeaders()}");

      patchesRequest.Validate();

      // identify VSS projectUid and CustomerUid
      var tfaHelper = new TagFileAuthHelper(LoggerFactory, ConfigStore, TagFileAuthProjectV5Proxy);
      var tfaResult = await tfaHelper.GetProjectUid(patchesRequest.ECSerial, patchesRequest.MachineLatitude, patchesRequest.MachineLongitude);

      if (tfaResult.Code != 0 || string.IsNullOrEmpty(tfaResult.ProjectUid) || string.IsNullOrEmpty(tfaResult.CustomerUid))
      {
        var errorMessage = $"Unable to identify a unique project or customer. Result: {JsonConvert.SerializeObject(tfaResult)}";
        Log.LogInformation(errorMessage);
        return BadRequest(new ContractExecutionResult(tfaResult.Code, errorMessage));
      }
      Log.LogInformation($"{nameof(GetSubGridPatches)}: tfaResult {JsonConvert.SerializeObject(tfaResult)}");

      // Set customerUid for downstream service calls e.g. ProjectSvc
      Log.LogInformation($"{nameof(GetSubGridPatches)}: requestHeaders {JsonConvert.SerializeObject(Request.Headers)} PrincipalCustomerUID {((RaptorPrincipal)User).CustomerUid} authNContext {JsonConvert.SerializeObject(((RaptorPrincipal)User).GetAuthNContext())}");
      if (((RaptorPrincipal)User).SetCustomerUid(tfaResult.CustomerUid))
        Request.Headers[HeaderConstants.X_VISION_LINK_CUSTOMER_UID] = tfaResult.CustomerUid;

      // this endpoint has no UserId so excludedSSs and targets are not relevant
      var filter = SetupCompactionFilter(Guid.Parse(tfaResult.ProjectUid), patchesRequest.BoundingBox);

      var liftSettings = SettingsManager.CompactionLiftBuildSettings(CompactionProjectSettings.DefaultSettings);
      Log.LogDebug($"{nameof(GetSubGridPatches)}: filter: {JsonConvert.SerializeObject(filter)} liftSettings: {JsonConvert.SerializeObject(liftSettings)}");

      var requestPatchId = 0;
      var requestPatchSize = 1000; // max # sub-grids to scan
      var requestIncludeTimeOffsets = true;
      var patchRequest = new PatchRequest(
        null, // obsolete
        Guid.Parse(tfaResult.ProjectUid),
        new Guid(),
        DisplayMode.Height,
        null,
        liftSettings,
        false,
        VolumesType.None,
        VelociraptorConstants.VOLUME_CHANGE_TOLERANCE,
        null, filter, null, FilterLayerMethod.AutoMapReset,
        requestPatchId, requestPatchSize, requestIncludeTimeOffsets);

      patchRequest.Validate();

      var v2PatchRequestResponse = await WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainerFactory
        .Build<CompactionSinglePatchExecutor>(LoggerFactory,
          ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
        .ProcessAsync(patchRequest));

      return Ok(v2PatchRequestResponse);
    }
  }
}
