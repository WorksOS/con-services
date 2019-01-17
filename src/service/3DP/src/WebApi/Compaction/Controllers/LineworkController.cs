using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
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
  /// Linework (DXF) file controller.
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class LineworkController : BaseController<LineworkController>
  {
    /// <inheritdoc />
    public LineworkController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileListProxy, settingsManager)
    { }

    /// <summary>
    /// Get all boundaries from provided linework (DXF) files.
    /// </summary>
    [HttpPost("api/v2/linework/boundaries")]
    public async Task<IActionResult> GetBoundariesFromLinework([FromServices] IRaptorFileUploadUtility fileUploadUtility, DxfFileRequest requestDto)
    {
      Log.LogDebug($"{nameof(GetBoundariesFromLinework)}: {requestDto}");

      var customerUid = ((RaptorPrincipal)Request.HttpContext.User).CustomerUid;

      var executorRequestObj = LineworkRequest
                               .Create(requestDto, customerUid)
                               .Validate();

      var (uploadSuccess, message) = fileUploadUtility.UploadFile(
        executorRequestObj.FileDescriptor, 
        customerUid,
        executorRequestObj.FileData);

      if (!uploadSuccess) return StatusCode((int)HttpStatusCode.BadRequest, message);

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory, RaptorClient, null, ConfigStore)
                         .ProcessAsync(executorRequestObj);

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, ((DxfLineworkFileResult)result).ConvertToGeoJson())
        : StatusCode((int)HttpStatusCode.BadRequest, result);
    }


    /// <summary>
    /// Gets a DXF linework representation of an alignment.
    /// </summary>
    /// <returns>A zipped file containing the linework and a result code and message</returns>
    [HttpGet("api/v2/linework/alignment")]
    public async Task<FileResult> GetLineworkFromAlignment(Guid projectUid, Guid alignmentUid, [FromServices] IPreferenceProxy prefProxy)
    {
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
        .ProcessAsync(request);

      var outputStream = new MemoryStream();
      using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
      {
        //Add metadata for result so can return any errors
        var metaDataEntry = zipArchive.CreateEntry("metadata.json");
        using (var stream = metaDataEntry.Open())
        {
          var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
          stream.Write(bytes, 0, bytes.Length);
        }
        //Add the DXF linework
        var alignResult = result as AlignmentLineworkResult;
        if (alignResult.DxfData != null)
        {
          var suffix = FileUtils.GeneratedFileSuffix(ImportedFileType.Alignment);
          string generatedName =
            FileUtils.GeneratedFileName(designDescriptor.File.FileName, suffix, FileUtils.DXF_FILE_EXTENSION);
          var dxfZipEntry = zipArchive.CreateEntry(generatedName);
          using (var stream = dxfZipEntry.Open())
          {
            alignResult.DxfData.CopyTo(stream);
          }
        }
      }
      // Don't forget to seek back, or else the content length will be 0
      outputStream.Seek(0, SeekOrigin.Begin);
      return new FileStreamResult(outputStream, "application/zip");
    }

  }
}
