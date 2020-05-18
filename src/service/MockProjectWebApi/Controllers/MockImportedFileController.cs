using System;
using System.Collections.Generic;
using System.Linq;
using CCSS.CWS.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

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
    [Route("api/v6/importedfiles")]
    [HttpGet]
    public FileDataResult GetMockImportedFiles([FromQuery] Guid projectUid, [FromQuery] bool getProjectCalibrationFiles = false)
    {
      Logger.LogInformation($"{nameof(GetMockImportedFiles)}: projectUid={projectUid} getProjectCalibrationFiles={getProjectCalibrationFiles}");

      var result = new FileDataResult();

      if (getProjectCalibrationFiles)
      {
        ImportedFilesService.ProjectConfigFiles.TryGetValue(projectUid.ToString(), out var projConfigFileList);
        result.ProjectConfigFileDescriptors = projConfigFileList;
      }
      else
      {
        ImportedFilesService.ImportedFiles.TryGetValue(projectUid.ToString(), out var fileList);
        result.ImportedFileDescriptors = fileList ?? new List<FileData>();
      }

      return result;
    }

    /// <summary>
    /// Used as a callback by Flow.JS
    /// </summary>
    [Route("api/v6/importedfile")]
    [HttpGet]
    public ActionResult Upload()
    {
      return new NoContentResult();
    }

    [Route("api/v6/importedfile")]
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
        var result = new FileDataSingleResult();
        if (ProjectConfigurationFileHelper.IsCwsFileType(importedFileType))
        {
          if (ProjectConfigurationFileHelper.IsSiteCollectorType(importedFileType, file.flowFilename))
          {
            result.ProjectConfigFileDescriptor = ImportedFilesService.ProjectConfigFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
              .SingleOrDefault(f => f.SiteCollectorFileName.Equals(file.flowFilename, StringComparison.OrdinalIgnoreCase));
          }
          else
          {
            result.ProjectConfigFileDescriptor = ImportedFilesService.ProjectConfigFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
              .SingleOrDefault(f => f.FileName.Equals(file.flowFilename, StringComparison.OrdinalIgnoreCase));
          }
        }
        else
        {
          result.ImportedFileDescriptor = ImportedFilesService.ImportedFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
            .SingleOrDefault(f => f.Name.Equals(file.flowFilename, StringComparison.OrdinalIgnoreCase));
        }

        return result;
      }

      return new FileDataSingleResult
      {
        Code = ContractExecutionStatesEnum.InternalProcessingError, 
        Message = "Failed to create imported file"
      };
    }

    [Route("api/v6/importedfile")]
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
        var result = new FileDataSingleResult();
        if (ProjectConfigurationFileHelper.IsCwsFileType(importedFileType))
        {
          if (ProjectConfigurationFileHelper.IsSiteCollectorType(importedFileType, file.flowFilename))
          {
            result.ProjectConfigFileDescriptor = ImportedFilesService.ProjectConfigFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
              .SingleOrDefault(f => f.SiteCollectorFileName.Equals(file.flowFilename, StringComparison.OrdinalIgnoreCase));
          }
          else
          {
            result.ProjectConfigFileDescriptor = ImportedFilesService.ProjectConfigFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
              .SingleOrDefault(f => f.FileName.Equals(file.flowFilename, StringComparison.OrdinalIgnoreCase));
          }
        }
        else
        {
          result.ImportedFileDescriptor = ImportedFilesService.ImportedFiles[ConstantsUtil.DIMENSIONS_PROJECT_UID]
            .SingleOrDefault(f => f.Name.Equals(file.flowFilename, StringComparison.OrdinalIgnoreCase));
        }

        return result;
      }

      return new FileDataSingleResult
      {
        Code = ContractExecutionStatesEnum.InternalProcessingError,
        Message = "Failed to update imported file"
      };
    }

    [Route("api/v6/importedfile")]
    [HttpDelete]
    public BaseDataResult DeleteMockImportedFile(
      [FromQuery] Guid projectUid, 
      [FromQuery] Guid? importedFileUid,
      [FromQuery] ImportedFileType? importedFileType,
      [FromQuery] string filename)
    {
      Logger.LogInformation($"DeleteMockImportedFile. projectUid {projectUid} importedFileUid: {importedFileUid} importedFileType: {importedFileType} filename: {filename}");

      return new BaseDataResult();
    }
  }
}
