using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which Updates an importedFile
  /// 
  /// For TRex, the file is stored on S3 and notified to Trex via a 3dp notification (1 for add and another for update)
  ///        Min/max zoom will not be determined this way for TRex-only (todo Elspeth?)
  ///        It continues to update the FileDescription to the DB
  ///
  /// </summary>
  public class UpdateImportedFileExecutor : RequestExecutorContainer
  {

    protected static ProjectErrorCodesProvider ProjectErrorCodesProvider = new ProjectErrorCodesProvider();

    /// <summary>
    /// Processes the Upsert
    /// </summary>  
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var importedFile = CastRequestObjectTo<UpdateImportedFile>(item, errorCode: 68);

      var existingImportedFile = await projectRepo.GetImportedFile(importedFile.ImportedFileUid.ToString());
      if (existingImportedFile == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(122),
            ProjectErrorCodesProvider.FirstNameWithOffset(122)));

      if (importedFile.IsDesignFileType)
      {
        await ImportedFileRequestHelper.NotifyTRexUpdateFile(importedFile.ProjectUid,
          importedFile.ImportedFileType, importedFile.FileDescriptor.FileName, importedFile.ImportedFileUid,
          importedFile.SurveyedUtc,
          log, customHeaders, serviceExceptionHandler,
          tRexImportFileProxy);
      }

      if (importedFile.ImportedFileType == ImportedFileType.Linework || 
          importedFile.ImportedFileType == ImportedFileType.GeoTiff)
      {
        string dxfFileName = null;
        if (importedFile.ImportedFileType == ImportedFileType.Linework)
          dxfFileName = importedFile.DataOceanFileName;
        //Generate raster tiles
        if (importedFile.ImportedFileType == ImportedFileType.Linework)
        {
          var projectTask = await ProjectRequestHelper.GetProject(importedFile.ProjectUid, new Guid(customerUid), new Guid(userId), log, serviceExceptionHandler, cwsProjectClient, customHeaders);
          dxfFileName = DataOceanFileUtil.DataOceanFileName(projectTask.CoordinateSystemFileName, false, Guid.Parse(projectTask.ProjectUID), null);
        }

        var jobRequest = TileGenerationRequestHelper.CreateRequest(
          importedFile.ImportedFileType,
          customerUid,
          importedFile.ProjectUid.ToString(),
          existingImportedFile.ImportedFileUid,
          importedFile.DataOceanRootFolder,
          importedFile.DataOceanFileName,
          dxfFileName,
          importedFile.DxfUnitsTypeId,
          importedFile.SurveyedUtc);
        await schedulerProxy.ScheduleVSSJob(jobRequest, customHeaders);
      }
      
      // if all succeeds, update Db and  put update to kafka que
      var updateImportedFileEvent = await ImportedFileRequestDatabaseHelper.UpdateImportedFileInDb(existingImportedFile,
          existingImportedFile.FileDescriptor,
          importedFile.SurveyedUtc, existingImportedFile.MinZoomLevel, existingImportedFile.MaxZoomLevel,
          importedFile.FileCreatedUtc, importedFile.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo);
      
      var fileDescriptor = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestDatabaseHelper.GetImportedFileList(importedFile.ProjectUid.ToString(), log, userId, projectRepo))
        .ToImmutableList()
        .FirstOrDefault(f => f.ImportedFileUid == importedFile.ImportedFileUid.ToString())
      );

      log.LogInformation(
        $"UpdateImportedFileExecutor. entry {(fileDescriptor.ImportedFileDescriptor == null ? "not " : "")}retrieved from DB : {JsonConvert.SerializeObject(fileDescriptor)}");

      return fileDescriptor;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
