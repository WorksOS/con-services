using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

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
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var importedFile = CastRequestObjectTo<CreateImportedFile>(item, errorCode: 68);

      await ImportedFileRequestDatabaseHelper.CheckIfParentSurfaceExistsAsync(importedFile.ImportedFileType, importedFile.ParentUid, serviceExceptionHandler, projectRepo);

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"), out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"),
        out var useRaptorGatewayDesignImport);

      // need to write to Db prior to 
      //      notifying raptor, as raptor needs the legacyImportedFileID 
      //      notifying TRex as Trex needs the ImportedFileUid
      var createImportedFileEvent = await ImportedFileRequestDatabaseHelper.CreateImportedFileinDb(
          Guid.Parse(customerUid),
          importedFile.ProjectUid,
          importedFile.ImportedFileType, importedFile.DxfUnitsType, importedFile.FileName,
          importedFile.SurveyedUtc, JsonConvert.SerializeObject(importedFile.FileDescriptor),
          importedFile.FileCreatedUtc, importedFile.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo, importedFile.ParentUid, importedFile.Offset,
          importedFile.ImportedFileUid)
        .ConfigureAwait(false);

      if (useTrexGatewayDesignImport && importedFile.IsDesignFileType)
      {
        await ImportedFileRequestHelper.NotifyTRexAddFile(importedFile.ProjectUid,
            importedFile.ImportedFileType, importedFile.FileName, createImportedFileEvent.ImportedFileUID,
            importedFile.SurveyedUtc,
            log, customHeaders, serviceExceptionHandler,
            tRexImportFileProxy, projectRepo)
          .ConfigureAwait(false);
      }

      if (useRaptorGatewayDesignImport && importedFile.ImportedFileType != ImportedFileType.GeoTiff)
      {
        var project =
          await ProjectRequestHelper.GetProject(importedFile.ProjectUid.ToString(), customerUid, log,
            serviceExceptionHandler, projectRepo);

        if (importedFile.ImportedFileType == ImportedFileType.DesignSurface ||
            importedFile.ImportedFileType == ImportedFileType.SurveyedSurface)
        {
          await ImportedFileRequestHelper.NotifyRaptorAddFile(project.ShortRaptorProjectId,
            importedFile.ProjectUid,
            importedFile.ImportedFileType, importedFile.DxfUnitsType, importedFile.FileDescriptor,
            createImportedFileEvent.ImportedFileID, createImportedFileEvent.ImportedFileUID, true,
            log, customHeaders, serviceExceptionHandler, productivity3dV2ProxyNotification, projectRepo);
        }

        var dxfFileName = importedFile.DataOceanFileName;

        if (importedFile.ImportedFileType == ImportedFileType.Alignment)
        {
          //Create DXF file for alignment center line
          dxfFileName = await ImportedFileRequestHelper.CreateGeneratedDxfFile(
            customerUid, importedFile.ProjectUid, createImportedFileEvent.ImportedFileUID, productivity3dV2ProxyCompaction, customHeaders, log,
            serviceExceptionHandler, authn, dataOceanClient, configStore, importedFile.DataOceanFileName, importedFile.DataOceanRootFolder);
        }

        var existing = await projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
          .ConfigureAwait(false);

        if (importedFile.ImportedFileType == ImportedFileType.Alignment ||
            importedFile.ImportedFileType == ImportedFileType.Linework)
        {
          //Generate raster tiles
          var jobRequest = TileGenerationRequestHelper.CreateRequest(
            importedFile.ImportedFileType,
            customerUid,
            importedFile.ProjectUid.ToString(),
            existing.ImportedFileUid,
            importedFile.DataOceanRootFolder,
            dxfFileName,
            DataOceanFileUtil.DataOceanFileName(project.CoordinateSystemFileName, false, Guid.Parse(project.ProjectUID), null),
            importedFile.DxfUnitsType,
            importedFile.SurveyedUtc);
          await schedulerProxy.ScheduleVSSJob(jobRequest, customHeaders);
        }
      }

      if (importedFile.ImportedFileType == ImportedFileType.GeoTiff)
      {
        var project = ProjectRequestHelper.GetProject(importedFile.ProjectUid.ToString(), customerUid, log, serviceExceptionHandler, projectRepo);

        var existing = projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());

        await Task.WhenAll(project, existing);

        var jobRequest = TileGenerationRequestHelper.CreateRequest(
          importedFile.ImportedFileType,
          customerUid,
          importedFile.ProjectUid.ToString(),
          existing.Result.ImportedFileUid,
          importedFile.DataOceanRootFolder,
          importedFile.DataOceanFileName,
          null,
          importedFile.DxfUnitsType,
          importedFile.SurveyedUtc);
        await schedulerProxy.ScheduleVSSJob(jobRequest, customHeaders);
      }
      
      var fileDescriptor = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestDatabaseHelper
          .GetImportedFileList(importedFile.ProjectUid.ToString(), log, userId, projectRepo)
          .ConfigureAwait(false))
        .ToImmutableList()
        .First(f => f.ImportedFileUid == createImportedFileEvent.ImportedFileUID.ToString())
      );

      log.LogInformation(
        $"CreateImportedFileV4. completed successfully. Response: {JsonConvert.SerializeObject(fileDescriptor)}");
      return fileDescriptor;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
