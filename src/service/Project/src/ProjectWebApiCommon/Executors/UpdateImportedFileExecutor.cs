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
      var updateImportedFile = CastRequestObjectTo<UpdateImportedFile>(item, errorCode: 68);

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"), out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"), out var useRaptorGatewayDesignImport);

      var existingImportedFile = await projectRepo.GetImportedFile(updateImportedFile.ImportedFileUid.ToString());
      if (existingImportedFile == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(122),
            ProjectErrorCodesProvider.FirstNameWithOffset(122)));

      if (useTrexGatewayDesignImport && updateImportedFile.IsDesignFileType)
      {
        await ImportedFileRequestHelper.NotifyTRexUpdateFile(updateImportedFile.ProjectUid,
          updateImportedFile.ImportedFileType, updateImportedFile.FileDescriptor.FileName, updateImportedFile.ImportedFileUid,
          updateImportedFile.SurveyedUtc,
          log, customHeaders, serviceExceptionHandler,
          tRexImportFileProxy, projectRepo);
      }

      if (useRaptorGatewayDesignImport && updateImportedFile.ImportedFileType != ImportedFileType.GeoTiff)
      {
        await ImportedFileRequestHelper.NotifyRaptorAddFile(
                                         updateImportedFile.LegacyProjectId, Guid.Parse(updateImportedFile.ProjectUid.ToString()),
                                         updateImportedFile.ImportedFileType, updateImportedFile.DxfUnitsTypeId,
                                         updateImportedFile.FileDescriptor, updateImportedFile.ImportedFileId,
                                         Guid.Parse(updateImportedFile.ImportedFileUid.ToString()), false, log, customHeaders,
                                         serviceExceptionHandler, raptorProxy,
                                         projectRepo);

        var dxfFileName = updateImportedFile.DataOceanFileName;
        if (updateImportedFile.ImportedFileType == ImportedFileType.Alignment)
        {
          //Create DXF file for alignment center line
          dxfFileName = await ImportedFileRequestHelper.CreateGeneratedDxfFile(
            customerUid, updateImportedFile.ProjectUid, updateImportedFile.ImportedFileUid, raptorProxy, customHeaders, log,
            serviceExceptionHandler, authn, dataOceanClient, configStore, updateImportedFile.DataOceanFileName, updateImportedFile.DataOceanRootFolder);
        }

        if (updateImportedFile.ImportedFileType == ImportedFileType.Alignment ||
            updateImportedFile.ImportedFileType == ImportedFileType.Linework)
        {
          //Generate raster tiles
          var projectTask = await ProjectRequestHelper.GetProject(updateImportedFile.ProjectUid.ToString(), customerUid, log, serviceExceptionHandler, projectRepo);

          var jobRequest = TileGenerationRequestHelper.CreateRequest(
            updateImportedFile.ImportedFileType, 
            customerUid, 
            updateImportedFile.ProjectUid.ToString(),
            existingImportedFile.ImportedFileUid, 
            updateImportedFile.DataOceanRootFolder, 
            dxfFileName,
            DataOceanFileUtil.DataOceanFileName(projectTask.CoordinateSystemFileName, false, Guid.Parse(projectTask.ProjectUID), null),
            updateImportedFile.DxfUnitsTypeId,
            updateImportedFile.SurveyedUtc);
          await schedulerProxy.ScheduleVSSJob(jobRequest, customHeaders);
        }
      }

      if (updateImportedFile.ImportedFileType == ImportedFileType.GeoTiff)
      {
        //Generate raster tiles

        var jobRequest = TileGenerationRequestHelper.CreateRequest(
          updateImportedFile.ImportedFileType,
          customerUid,
          updateImportedFile.ProjectUid.ToString(),
          existingImportedFile.ImportedFileUid,
          updateImportedFile.DataOceanRootFolder, 
          updateImportedFile.DataOceanFileName,
          null,
          updateImportedFile.DxfUnitsTypeId,
          updateImportedFile.SurveyedUtc);
        await schedulerProxy.ScheduleVSSJob(jobRequest, customHeaders);
      }


      // if all succeeds, update Db and  put update to kafka que
      var updateImportedFileEvent = await ImportedFileRequestDatabaseHelper.UpdateImportedFileInDb(existingImportedFile,
          existingImportedFile.FileDescriptor,
          updateImportedFile.SurveyedUtc, existingImportedFile.MinZoomLevel, existingImportedFile.MaxZoomLevel,
          updateImportedFile.FileCreatedUtc, updateImportedFile.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo);

      var messagePayload = JsonConvert.SerializeObject(new { UpdateImportedFileEvent = updateImportedFileEvent });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestDatabaseHelper.GetImportedFileList(updateImportedFile.ProjectUid.ToString(), log, userId, projectRepo).ConfigureAwait(false))
        .ToImmutableList()
        .FirstOrDefault(f => f.ImportedFileUid == updateImportedFile.ImportedFileUid.ToString())
      );

      log.LogInformation(
        $"UpdateImportedFileExecutor. entry {(importedFile.ImportedFileDescriptor == null ? "not " : "")}retrieved from DB : {JsonConvert.SerializeObject(importedFile)}");

      return importedFile;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
