using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Line work file controller.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class LineworkController : BaseController<LineworkController>
  {
    /// <inheritdoc />
    public LineworkController(IConfigurationStore configStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileImportProxy, settingsManager)
    { }

    /// <summary>
    /// Get all boundaries from provided line work (DXF) file.
    /// </summary>
    [HttpPost("api/v2/linework/boundaries")]
    public async Task<IActionResult> GetBoundariesFromLinework([FromForm] DxfFileRequest requestDto)
    {
      Log.LogDebug($"{nameof(GetBoundariesFromLinework)}: {requestDto}");

      var executorRequestObj = new LineworkRequest(requestDto).Validate();

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory,
                         configStore: ConfigStore)
                         .ProcessAsync(executorRequestObj);

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, ((DxfLineworkFileResult)result).ConvertToGeoJson(requestDto.ConvertLineStringCoordsToPolygon, requestDto.MaxVerticesPerBoundary))
        : StatusCode((int)HttpStatusCode.BadRequest, result);
    }

    /// <summary>
    /// Gets a DXF line work representation of an alignment.
    /// </summary>
    /// <returns>A zipped file containing the line work and a result code and message</returns>
    [HttpGet("api/v2/linework/alignment")]
    public async Task<FileResult> GetLineworkFromAlignment([FromQuery] Guid projectUid, [FromQuery] Guid alignmentUid, [FromServices] IPreferenceProxy prefProxy)
    {
      Log.LogDebug($"{nameof(GetLineworkFromAlignment)}: projectUid={projectUid} alignmentUid={alignmentUid}");

#if RAPTOR
      var projectId = GetLegacyProjectId(projectUid);
      var designDescriptor = GetAndValidateDesignDescriptor(projectUid, alignmentUid, OperationType.GeneratingDxf);
      var userPreferences = prefProxy.GetUserPreferences(CustomHeaders);

      await Task.WhenAll(projectId, designDescriptor, userPreferences);

      var dxfUnitsType = DxfUnitsType.Meters;

      switch (userPreferences.Result.Units.UnitsType())
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

      var request = AlignmentLineworkRequest.Create(projectUid, projectId.Result, designDescriptor.Result.File, dxfUnitsType);
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
          FileUtils.GeneratedFileName(designDescriptor.Result.File.FileName, suffix, FileUtils.DXF_FILE_EXTENSION);
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
