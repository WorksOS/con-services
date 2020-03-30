using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockImportedFileController : BaseController
  {
    private readonly ImportedFilesService ImportedFilesService;

    public MockImportedFileController(ILoggerFactory loggerFactory, IImportedFilesService importedFilesService)
      : base(loggerFactory)
    {
      ImportedFilesService = (ImportedFilesService)importedFilesService;
    }

    /// <summary>
    /// Gets the list of imported files used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked imported files</returns>
    [Route("api/v4/importedfiles")]
    [HttpGet]
    public FileDataResult GetMockImportedFiles([FromQuery] Guid projectUid)
    {
      Logger.LogInformation($"{nameof(GetMockImportedFiles)}: projectUid={projectUid}");

      ImportedFilesService.ImportedFiles.TryGetValue(projectUid.ToString(), out var fileList);

      return new FileDataResult
      {
        ImportedFileDescriptors = fileList ?? new List<FileData>()
      };
    }

    /// <summary>
    /// Used as a callback by Flow.JS
    /// </summary>
    [Route("api/v4/importedfile")]
    [HttpGet]
    public ActionResult Upload()
    {
      return new NoContentResult();
    }

    [Route("api/v4/importedfile")]
    [HttpPost]
    [ActionName("Upload")]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1000000000)]
    public FileDataSingleResult CreateMockImportedFile(
      FlowFile file,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc = null)
    {
      Logger.LogInformation(
        $"CreateMockImportedFile. file: {file.flowFilename} path {file.path} projectUid {projectUid} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (projectUid.ToString() == ConstantsUtil.DIMENSIONS_PROJECT_UID)
      {
        return new FileDataSingleResult
        {
          ImportedFileDescriptor = ImportedFilesService.ImportedFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
                                                       .SingleOrDefault(f => f.Name.Equals(file.flowFilename, StringComparison.OrdinalIgnoreCase))
        };
      }

      return new FileDataSingleResult { Code = VSS.MasterData.Models.ResultHandling.Abstractions.ContractExecutionStatesEnum.InternalProcessingError, Message = "Failed to create imported file" };
    }

    [Route("api/v4/importedfile")]
    [HttpPut]
    [ActionName("Upload")]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1000000000)]
    public FileDataSingleResult UpdateMockImportedFile(
      FlowFile file,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc = null)
    {
      Logger.LogInformation(
        $"UpdateMockImportedFile. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (projectUid.ToString() == ConstantsUtil.DIMENSIONS_PROJECT_UID)
      {
        return new FileDataSingleResult
        {
          ImportedFileDescriptor = ImportedFilesService.ImportedFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
                                                       .SingleOrDefault(f => f.Name == file.flowFilename)
        };
      }

      return new FileDataSingleResult
      {
        Code = VSS.MasterData.Models.ResultHandling.Abstractions.ContractExecutionStatesEnum.InternalProcessingError,
        Message = "Failed to update imported file"
      };
    }

    [Route("api/v4/importedfile")]
    [HttpDelete]
    public BaseDataResult DeleteMockImportedFile([FromQuery] Guid projectUid, [FromQuery] Guid importedFileUid)
    {
      Logger.LogInformation($"DeleteMockImportedFile. projectUid {projectUid} importedFileUid: {importedFileUid}");
      return new BaseDataResult();
    }
  }
}
