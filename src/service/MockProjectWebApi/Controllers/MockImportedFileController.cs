using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Services;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockImportedFileController
  {
    private readonly ImportedFilesService importedFilesService;

    public MockImportedFileController(IImportedFilesService importedFilesService)
    {
      this.importedFilesService = (ImportedFilesService)importedFilesService;
    }

    /// <summary>
    /// Gets the list of imported files used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked imported files</returns>
    [HttpGet("api/v4/mock/importedfiles")]
    public FileDataResult GetMockImportedFiles([FromQuery] Guid projectUid)
    {
      Console.WriteLine($"{nameof(GetMockImportedFiles)}: projectUid={projectUid}");

      importedFilesService.ImportedFiles.TryGetValue(projectUid.ToString(), out var fileList);

      return new FileDataResult
      {
        ImportedFileDescriptors = fileList ?? new List<FileData>()
      };
    }

    /// <summary>
    /// Used as a callback by Flow.JS
    /// </summary>
    [HttpGet("api/v4/importedfile")]
    public ActionResult Upload()
    {
      return new NoContentResult();
    }

    [HttpPost("api/v4/mock/importedfile")]
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
      Console.WriteLine(
        $"CreateMockImportedFile. file: {file.flowFilename} path {file.path} projectUid {projectUid} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (projectUid.ToString() == ConstantsUtil.DIMENSIONS_PROJECT_UID)
      {
        return new FileDataSingleResult
        {
          ImportedFileDescriptor = importedFilesService.ImportedFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
                                                       .SingleOrDefault(f => f.Name.Equals(file.flowFilename, StringComparison.OrdinalIgnoreCase))
        };
      }

      return new FileDataSingleResult { Code = VSS.MasterData.Models.ResultHandling.Abstractions.ContractExecutionStatesEnum.InternalProcessingError, Message = "Failed to create imported file" };
    }

    [HttpPut("api/v4/mock/importedfile")]
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
      Console.WriteLine(
        $"UpdateMockImportedFile. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (projectUid.ToString() == ConstantsUtil.DIMENSIONS_PROJECT_UID)
      {
        return new FileDataSingleResult
        {
          ImportedFileDescriptor = importedFilesService.ImportedFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
                                                       .SingleOrDefault(f => f.Name == file.flowFilename)
        };
      }

      return new FileDataSingleResult
      {
        Code = VSS.MasterData.Models.ResultHandling.Abstractions.ContractExecutionStatesEnum.InternalProcessingError,
        Message = "Failed to update imported file"
      };
    }

    [HttpDelete("api/v4/mock/importedfile")]
    public BaseDataResult DeleteMockImportedFile([FromQuery] Guid projectUid, [FromQuery] Guid importedFileUid)
    {
      Console.WriteLine($"DeleteMockImportedFile. projectUid {projectUid} importedFileUid: {importedFileUid}");
      return new BaseDataResult();
    }
  }
}
