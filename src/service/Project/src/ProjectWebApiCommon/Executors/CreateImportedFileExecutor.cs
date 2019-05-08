﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Scheduler.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// Do the import (creation) of a file uploaded directly, or via scheduler 
  /// This can be called by the background upload
  ///      (file stored in TEMPRORAY vs-exports- bucket in AWS, then re downloaded with scheduler request),
  ///      or Synchronise upload (file stored locally)
  ///
  /// For Raptor, the file is stored on TCC and notified to Raptor via a 3dp notification (1 for add/update)
  ///        Min/Max zoom is returned from 3dp
  /// For TRex, the file is stored on S3 and notified to Trex via a 3dp notification (1 for add and another for update)
  ///        Min/max zoom will not be determined this way for TRex-only (todo Elspeth?)
  ///        It continues to write a FileDescription to the DB,
  ///              even though the tcc-specific filespaceID and path are not required for TRex.
  ///              I decided to leave this for now as s3 is probably not the final storage medium,
  ///              that will probably be DataOcean, and requirements are not known yet.
  ///
  /// </summary>
  /// <returns>Details of the upload file</returns>
  public class CreateImportedFileExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// Adds file via Raptor and/or Trex
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var createimportedfile = item as CreateImportedFile;
      if (createimportedfile == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "shouldn't get here"); // to keep compiler happy
      }

      await ImportedFileRequestDatabaseHelper.CheckIfParentSurfaceExistsAsync(createimportedfile.ImportedFileType, createimportedfile.ParentUid, serviceExceptionHandler, projectRepo);

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"), out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"),
        out var useRaptorGatewayDesignImport);
      var isDesignFileType = createimportedfile.ImportedFileType == ImportedFileType.DesignSurface ||
                             createimportedfile.ImportedFileType == ImportedFileType.SurveyedSurface ||
                             createimportedfile.ImportedFileType == ImportedFileType.Alignment ||
                             createimportedfile.ImportedFileType == ImportedFileType.ReferenceSurface;

      // need to write to Db prior to 
      //      notifying raptor, as raptor needs the legacyImportedFileID 
      //      notifying TRex as Trex needs the ImportedFileUid
      var createImportedFileEvent = await ImportedFileRequestDatabaseHelper.CreateImportedFileinDb(
          Guid.Parse(customerUid),
          createimportedfile.ProjectUid,
          createimportedfile.ImportedFileType, createimportedfile.DxfUnitsType, createimportedfile.FileName,
          createimportedfile.SurveyedUtc, JsonConvert.SerializeObject(createimportedfile.FileDescriptor),
          createimportedfile.FileCreatedUtc, createimportedfile.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo, createimportedfile.ParentUid, createimportedfile.Offset)
        .ConfigureAwait(false);

      if (useTrexGatewayDesignImport && isDesignFileType)
      {
        var result = await ImportedFileRequestHelper.NotifyTRexAddFile(createimportedfile.ProjectUid,
            createimportedfile.ImportedFileType, createimportedfile.FileName, createImportedFileEvent.ImportedFileUID,
            createimportedfile.SurveyedUtc,
            log, customHeaders, serviceExceptionHandler,
            tRexImportFileProxy, projectRepo)
          .ConfigureAwait(false);
      }

      if (useRaptorGatewayDesignImport)
      {
        var project =
          await ProjectRequestHelper.GetProject(createimportedfile.ProjectUid.ToString(), customerUid, log,
            serviceExceptionHandler, projectRepo);

        var addFileResult = await ImportedFileRequestHelper.NotifyRaptorAddFile(project.LegacyProjectID,
          createimportedfile.ProjectUid,
          createimportedfile.ImportedFileType, createimportedfile.DxfUnitsType, createimportedfile.FileDescriptor,
          createImportedFileEvent.ImportedFileID, createImportedFileEvent.ImportedFileUID, true,
          log, customHeaders, serviceExceptionHandler, raptorProxy, projectRepo).ConfigureAwait(false);

        var dxfFileName = createimportedfile.FileName;
        if (createimportedfile.ImportedFileType == ImportedFileType.Alignment)
        {
          //Create DXF file for alignment center line
          dxfFileName = await ImportedFileRequestHelper.CreateGeneratedDxfFile(
            customerUid, createimportedfile.ProjectUid, createImportedFileEvent.ImportedFileUID, raptorProxy, customHeaders, log,
            serviceExceptionHandler, authn, dataOceanClient, configStore, createimportedfile.FileName, createimportedfile.DataOceanRootFolder);
        }

        var existing = await projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
          .ConfigureAwait(false);

        if (createimportedfile.ImportedFileType == ImportedFileType.Alignment ||
            createimportedfile.ImportedFileType == ImportedFileType.Linework)
        {
          //Generate DXF tiles
          var jobRequest = new JobRequest
          {
            JobUid = DxfTileGenerationJob.VSSJOB_UID,
            RunParameters = new DxfTileGenerationRequest
            {
              CustomerUid = Guid.Parse(customerUid),
              ProjectUid = createimportedfile.ProjectUid,
              ImportedFileUid = Guid.Parse(existing.ImportedFileUid),
              DataOceanRootFolder = createimportedfile.DataOceanRootFolder,
              DxfFileName = dxfFileName,
              DcFileName = project.CoordinateSystemFileName,
              DxfUnitsType = createimportedfile.DxfUnitsType
            }
          };
          await schedulerProxy.ScheduleVSSJob(jobRequest, customHeaders);
        }
      }

      var messagePayload = JsonConvert.SerializeObject(new {CreateImportedFileEvent = createImportedFileEvent});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(createImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestDatabaseHelper
          .GetImportedFileList(createimportedfile.ProjectUid.ToString(), log, userId, projectRepo)
          .ConfigureAwait(false))
        .ToImmutableList()
        .First(f => f.ImportedFileUid == createImportedFileEvent.ImportedFileUID.ToString())
      );

      log.LogInformation(
        $"CreateImportedFileV4. completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      return importedFile;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
