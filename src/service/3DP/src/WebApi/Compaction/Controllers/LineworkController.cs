using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
#if RAPTOR
using VSS.Productivity3D.Common.Algorithms;
#endif
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Linework file controller.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class LineworkController : BaseController<LineworkController>
  {
    /// <inheritdoc />
    public LineworkController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileListProxy, settingsManager)
    { }

    /// <summary>
    /// Get all boundaries from provided linework (DXF) file.
    /// </summary>
    [HttpPost("api/v2/linework/boundaries")]
    public async Task<IActionResult> GetBoundariesFromLinework([FromServices] IRaptorFileUploadUtility fileUploadUtility, [FromForm] DxfFileRequest requestDto)
    {
      Log.LogDebug($"{nameof(GetBoundariesFromLinework)}: {requestDto}");
#if RAPTOR
      var customerUid = ((RaptorPrincipal)Request.HttpContext.User).CustomerUid;
      var uploadPath = Path.Combine(ConfigStore.GetValueString("SHAREUNC"), "Temp", "LineworkFileUploads", customerUid);
      var executorRequestObj = new LineworkRequest(requestDto, uploadPath).Validate();

      var uploadResult = fileUploadUtility.UploadFile(executorRequestObj.DxfFileDescriptor, executorRequestObj.DxfFileData);
      if (!uploadResult.success) return StatusCode((int)HttpStatusCode.BadRequest, uploadResult.message);

      uploadResult = fileUploadUtility.UploadFile(executorRequestObj.CoordinateSystemFileDescriptor, executorRequestObj.CoordinateSystemFileData);
      if (!uploadResult.success) return StatusCode((int)HttpStatusCode.BadRequest, uploadResult.message);

      executorRequestObj.ClearFileData();

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory, RaptorClient, configStore: ConfigStore)
                         .ProcessAsync(executorRequestObj);

      fileUploadUtility.DeleteFile(Path.Combine(executorRequestObj.DxfFileDescriptor.Path, executorRequestObj.DxfFileDescriptor.FileName));
      fileUploadUtility.DeleteFile(Path.Combine(executorRequestObj.CoordinateSystemFileDescriptor.Path, executorRequestObj.CoordinateSystemFileDescriptor.FileName));

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, ((DxfLineworkFileResult)result).ConvertToGeoJson(requestDto.ConvertLineStringCoordsToPolygon, requestDto.MaxPoints))
        : StatusCode((int)HttpStatusCode.BadRequest, result);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// Gets a DXF linework representation of an alignment.
    /// </summary>
    /// <returns>A zipped file containing the linework and a result code and message</returns>
    [HttpGet("api/v2/linework/alignment")]
    public async Task<FileResult> GetLineworkFromAlignment([FromQuery] Guid projectUid, [FromQuery] Guid alignmentUid, [FromServices] IPreferenceProxy prefProxy)
    {
      Log.LogDebug($"{nameof(GetLineworkFromAlignment)}: projectUid={projectUid} alignmentUid={alignmentUid}");

#if RAPTOR
      var projectId = await GetLegacyProjectId(projectUid);
      var designDescriptor = await GetAndValidateDesignDescriptor(projectUid, alignmentUid, OperationType.GeneratingDxf);
      var dxfUnitsType = DxfUnitsType.Meters;
      var userPreferences = await prefProxy.GetUserPreferences(CustomHeaders);
      switch (userPreferences.Units.UnitsType())
      {
        case UnitsTypeEnum.Metric:
          dxfUnitsType = DxfUnitsType.Meters;
          break;
        case UnitsTypeEnum.Imperial:
          dxfUnitsType = DxfUnitsType.ImperialFeet;
          break;
        case UnitsTypeEnum.US:
          dxfUnitsType = DxfUnitsType.UsSurveyFeet;
          break;
      }

      var request = AlignmentLineworkRequest.Create(projectUid, projectId, designDescriptor.File, dxfUnitsType);
      var result = await RequestExecutorContainerFactory
        .Build<AlignmentLineworkExecutor>(LoggerFactory, RaptorClient, null, ConfigStore)
        .ProcessAsync(request) as AlignmentLineworkResult;

      var outputStream = new MemoryStream();
      using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
      {
        //Note: When all DXF tile generation stuff removed this can be changed as the project
        //service creates its own generated name. It doesn't use the one from the zip file.
        var suffix = FileUtils.GeneratedFileSuffix(ImportedFileType.Alignment);
        string generatedName =
          FileUtils.GeneratedFileName(designDescriptor.File.FileName, suffix, FileUtils.DXF_FILE_EXTENSION);
        var dxfZipEntry = zipArchive.CreateEntry(generatedName);
        using (var stream = dxfZipEntry.Open())
        {
          result.DxfData?.CopyTo(stream);
        }
      }

      // Don't forget to seek back, or else the content length will be 0
      outputStream.Seek(0, SeekOrigin.Begin);
      return new FileStreamResult(outputStream, ContentTypeConstants.ApplicationZip);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }
  }
}
