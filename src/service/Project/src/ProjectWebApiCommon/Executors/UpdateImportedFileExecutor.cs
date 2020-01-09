using System;
using System.Collections.Generic;
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
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which Updates an importedFile
  /// 
  /// For Raptor, the file is stored on TCC and notified to Raptor via a 3dp notification (1 for add/update)
  ///        Min/Max zoom is returned from 3dp
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
      var tilesAreBeingGenerated = false;

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"), out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"), out var useRaptorGatewayDesignImport);

      var existingImportedFile = await projectRepo.GetImportedFile(importedFile.ImportedFileUid.ToString());
      if (existingImportedFile == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(122),
            ProjectErrorCodesProvider.FirstNameWithOffset(122)));

      if (useTrexGatewayDesignImport && importedFile.IsDesignFileType)
      {
        await ImportedFileRequestHelper.NotifyTRexUpdateFile(importedFile.ProjectUid,
          importedFile.ImportedFileType, importedFile.FileDescriptor.FileName, importedFile.ImportedFileUid,
          importedFile.SurveyedUtc,
          log, customHeaders, serviceExceptionHandler,
          tRexImportFileProxy);
      }

      if (useRaptorGatewayDesignImport && importedFile.ImportedFileType != ImportedFileType.GeoTiff)
      {
        if (importedFile.ImportedFileType == ImportedFileType.DesignSurface ||
            importedFile.ImportedFileType == ImportedFileType.SurveyedSurface)
        {
          await ImportedFileRequestHelper.NotifyRaptorAddFile(
            importedFile.LegacyProjectId, Guid.Parse(importedFile.ProjectUid.ToString()),
            importedFile.ImportedFileType, importedFile.DxfUnitsTypeId,
            importedFile.FileDescriptor, importedFile.ImportedFileId,
            Guid.Parse(importedFile.ImportedFileUid.ToString()), false, log, customHeaders,
            serviceExceptionHandler, productivity3dV2ProxyNotification,
            projectRepo);
        }

        var dxfFileName = importedFile.DataOceanFileName;
        if (importedFile.ImportedFileType == ImportedFileType.Alignment)
        {
          //Create DXF file for alignment center line
          dxfFileName = await ImportedFileRequestHelper.CreateGeneratedDxfFile(
            customerUid, importedFile.ProjectUid, importedFile.ImportedFileUid, productivity3dV2ProxyCompaction, customHeaders, log,
            serviceExceptionHandler, authn, dataOceanClient, configStore, importedFile.DataOceanFileName, importedFile.DataOceanRootFolder);
        }

        if (importedFile.ImportedFileType == ImportedFileType.Alignment ||
            importedFile.ImportedFileType == ImportedFileType.Linework)
        {
          //Generate raster tiles
          var projectTask = await ProjectRequestHelper.GetProject(importedFile.ProjectUid.ToString(), customerUid, log, serviceExceptionHandler, projectRepo);

          var jobRequest = TileGenerationRequestHelper.CreateRequest(
            importedFile.ImportedFileType,
            customerUid,
            importedFile.ProjectUid.ToString(),
            existingImportedFile.ImportedFileUid,
            importedFile.DataOceanRootFolder,
            dxfFileName,
            DataOceanFileUtil.DataOceanFileName(projectTask.CoordinateSystemFileName, false, Guid.Parse(projectTask.ProjectUID), null),
            importedFile.DxfUnitsTypeId,
            importedFile.SurveyedUtc);
          tilesAreBeingGenerated = true;
          await schedulerProxy.ScheduleVSSJob(jobRequest, customHeaders);
        }
      }

      if (importedFile.ImportedFileType == ImportedFileType.GeoTiff)
      {
        //Generate raster tiles

        var jobRequest = TileGenerationRequestHelper.CreateRequest(
          importedFile.ImportedFileType,
          customerUid,
          importedFile.ProjectUid.ToString(),
          existingImportedFile.ImportedFileUid,
          importedFile.DataOceanRootFolder,
          importedFile.DataOceanFileName,
          null,
          importedFile.DxfUnitsTypeId,
          importedFile.SurveyedUtc);
        tilesAreBeingGenerated = true;
        await schedulerProxy.ScheduleVSSJob(jobRequest, customHeaders);
      }

      // if all succeeds, update Db and  put update to kafka que
      var updateImportedFileEvent = await ImportedFileRequestDatabaseHelper.UpdateImportedFileInDb(existingImportedFile,
          existingImportedFile.FileDescriptor,
          importedFile.SurveyedUtc, existingImportedFile.MinZoomLevel, existingImportedFile.MaxZoomLevel,
          importedFile.FileCreatedUtc, importedFile.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo);

      var messagePayload = JsonConvert.SerializeObject(new { UpdateImportedFileEvent = updateImportedFileEvent });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      var fileDescriptor = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestDatabaseHelper.GetImportedFileList(importedFile.ProjectUid.ToString(), log, userId, projectRepo).ConfigureAwait(false))
        .ToImmutableList()
        .FirstOrDefault(f => f.ImportedFileUid == importedFile.ImportedFileUid.ToString())
      );

      // scheduler will generate projectEvent notification when complete. This covers e.g. SSurface
      if (!tilesAreBeingGenerated)
        await projectEventHubClient.FileImportIsComplete(new ImportedFileStatus(importedFile.ProjectUid, Guid.Parse(existingImportedFile.ImportedFileUid)));


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
