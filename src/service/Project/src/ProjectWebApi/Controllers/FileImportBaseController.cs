using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// FileImporter controller
  /// </summary>
  public class FileImportBaseController : BaseController<FileImportBaseController>
  {
    protected ITransferProxyFactory persistantTransferProxyFactory;
    protected IFilterServiceProxy filterServiceProxy;
    protected ITRexImportFileProxy tRexImportFileProxy;

    protected string FileSpaceId;
    protected string DataOceanRootFolderId;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IRequestFactory _requestFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileImportBaseController"/> class.
    /// </summary>
    public FileImportBaseController(IConfigurationStore config, ITransferProxyFactory transferProxyFactory,
      IFilterServiceProxy filterServiceProxy, ITRexImportFileProxy tRexImportFileProxy, IRequestFactory requestFactory)
    {
      this._requestFactory = requestFactory;

      this.persistantTransferProxyFactory = transferProxyFactory; //.NewProxy(TransferProxyType.DesignImport);
      this.filterServiceProxy = filterServiceProxy;
      this.tRexImportFileProxy = tRexImportFileProxy;

      ConfigStore = config;
      FileSpaceId = ConfigStore.GetValueString("TCCFILESPACEID");
      DataOceanRootFolderId = ConfigStore.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
    }

    /// <summary>
    /// Validates a project identifier.
    /// </summary>
    protected async Task ValidateProjectId(string projectUid)
    {
      LogCustomerDetails("GetProject", projectUid);
      var project =
        (await ProjectRequestHelper.GetProjectListForCustomer(new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient, null, null, true, false, customHeaders))
        .FirstOrDefault(p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));
      if (project == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      Logger.LogInformation($"Project {JsonConvert.SerializeObject(project)} retrieved");
    }

    /// <summary>
    /// Sets activated state for imported files.
    /// </summary>
    protected async Task<IEnumerable<Guid>> SetFileActivatedState(string projectUid, Dictionary<Guid, bool> fileUids)
    {
      Logger.LogDebug($"SetFileActivatedState: projectUid={projectUid}, {fileUids.Keys.Count} files with changed state");

      var deactivatedFileList = await ImportedFileRequestDatabaseHelper.GetImportedFileProjectSettings(projectUid, UserId, ProjectRepo).ConfigureAwait(false) ?? new List<ActivatedFileDescriptor>();
      Logger.LogDebug($"SetFileActivatedState: originally {deactivatedFileList.Count} deactivated files");

      var missingUids = new List<Guid>();
      foreach (var key in fileUids.Keys)
      {
        //fileUids contains only uids of files whose state has changed.
        //In the project settings we store only deactivated files.
        //Therefore if the value is true remove from the list else add to the list
        if (fileUids[key])
        {
          var item = deactivatedFileList.SingleOrDefault(d => d.ImportedFileUid == key.ToString());
          if (item != null)
          {
            deactivatedFileList.Remove(item);
          }
          else
          {
            missingUids.Add(key);
            Logger.LogInformation($"SetFileActivatedState: ImportFile '{key}' not found in project settings.");
          }
        }
        else
        {
          deactivatedFileList.Add(new ActivatedFileDescriptor { ImportedFileUid = key.ToString(), IsActivated = false });
        }
      }

      Logger.LogDebug($"SetFileActivatedState: now {deactivatedFileList.Count} deactivated files, {missingUids.Count} missingUids");

      var projectSettingsRequest =
        _requestFactory.Create<ProjectSettingsRequestHelper>(r => r
            .CustomerUid(CustomerUid))
          .CreateProjectSettingsRequest(projectUid, JsonConvert.SerializeObject(deactivatedFileList), ProjectSettingsType.ImportedFiles);
      projectSettingsRequest.Validate();

      _ = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpsertProjectSettingsExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, headers: customHeaders,
            productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction, 
            projectRepo: ProjectRepo, cwsProjectClient: CwsProjectClient)
          .ProcessAsync(projectSettingsRequest)
      ) as ProjectSettingsResult;

      var changedUids = fileUids.Keys.Except(missingUids);
      Logger.LogDebug($"SetFileActivatedState: {changedUids.Count()} changedUids");

      return changedUids;
    }

    protected bool IsTRexDesignFileType(ImportedFileType importedFileType)
    {
      return importedFileType == ImportedFileType.DesignSurface ||
             importedFileType == ImportedFileType.SurveyedSurface ||
             importedFileType == ImportedFileType.Alignment;
      //Don't save reference surface to s3 (The original design file will have been saved).
    }

    /// <summary>
    /// Common file processing method used by all importedFile endpoints.
    /// </summary>
    protected async Task<ImportedFileDescriptorSingleResult> UpsertFileInternal(
      string filename,
      Stream fileStream,
      Guid projectUid,
      ImportedFileType importedFileType,
      DxfUnitsType dxfUnitsType,
      DateTime fileCreatedUtc,
      DateTime fileUpdatedUtc,
      DateTime? surveyedUtc,
      ISchedulerProxy schedulerProxy,
      Guid? parentUid = null,
      double? offset = null)
    {
      ImportedFileDescriptorSingleResult importedFile = null;

      var existing = await ImportedFileRequestDatabaseHelper
        .GetImportedFileForProject
        (projectUid.ToString(), filename, importedFileType, surveyedUtc,
          Logger, ProjectRepo, offset, parentUid)
        .ConfigureAwait(false);

      var creating = existing == null;

      Logger.LogInformation(
        creating
          ? $"{nameof(UpsertFileInternal)}. file doesn't exist already in DB: {filename} projectUid {projectUid} ImportedFileType: {importedFileType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())} parentUid {parentUid} offset: {offset}"
          : $"{nameof(UpsertFileInternal)}. file exists already in DB. Will be updated: {JsonConvert.SerializeObject(existing)}");

      FileDescriptor fileDescriptor = null;

      var importedFileUid = creating ? Guid.NewGuid() : Guid.Parse(existing.ImportedFileUid);
      var dataOceanFileName = DataOceanFileUtil.DataOceanFileName(filename,
        importedFileType == ImportedFileType.SurveyedSurface || importedFileType == ImportedFileType.GeoTiff,
        importedFileUid, surveyedUtc);

      if (importedFileType == ImportedFileType.ReferenceSurface)
      {
        //FileDescriptor not used for reference surface but validation requires values
        fileDescriptor = FileDescriptor.CreateFileDescriptor("Not applicable", "Not applicable", filename);
      }
      else
      {
        if (IsTRexDesignFileType(importedFileType))
        {
          fileDescriptor = ProjectRequestHelper.WriteFileToS3Repository(
            fileStream, projectUid.ToString(), filename,
            importedFileType == ImportedFileType.SurveyedSurface, surveyedUtc,
            Logger, ServiceExceptionHandler, persistantTransferProxyFactory.NewProxy(TransferProxyType.DesignImport));
        }

        //This is needed for ATs.
        fileDescriptor = FileDescriptor.CreateFileDescriptor(
          FileSpaceId,
          $"/{CustomerUid}/{projectUid}",
          filename);

        if (importedFileType == ImportedFileType.Linework || importedFileType == ImportedFileType.GeoTiff)
        {
          //save copy to DataOcean      
          await DataOceanHelper.WriteFileToDataOcean(
              fileStream, DataOceanRootFolderId, CustomerUid, projectUid.ToString(), dataOceanFileName,
              Logger, ServiceExceptionHandler, DataOceanClient, Authorization, importedFileUid, ConfigStore);
        }
      }

      if (creating)
      {
        var createImportedFile = new CreateImportedFile(
          projectUid, filename, fileDescriptor, importedFileType, surveyedUtc, dxfUnitsType,
          fileCreatedUtc, fileUpdatedUtc, DataOceanRootFolderId, parentUid, offset, importedFileUid, dataOceanFileName);

        importedFile = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<CreateImportedFileExecutor>(
              LoggerFactory, ConfigStore, ServiceExceptionHandler, CustomerUid, UserId, UserEmailAddress, customHeaders,
              productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction,
              persistantTransferProxyFactory: persistantTransferProxyFactory, tRexImportFileProxy: tRexImportFileProxy,
              projectRepo: ProjectRepo, dataOceanClient: DataOceanClient, authn: Authorization, schedulerProxy: schedulerProxy,
              cwsProjectClient: CwsProjectClient)
            .ProcessAsync(createImportedFile)
        ) as ImportedFileDescriptorSingleResult;

        Logger.LogInformation(
          $"{nameof(UpsertFileInternal)}: Create completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      }
      else
      {
        // this also validates that this customer has access to the projectUid
        var project = await ProjectRequestHelper.GetProject(projectUid, new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders);

        var importedFileUpsertEvent = new UpdateImportedFile(
          projectUid, project.ShortRaptorProjectId, importedFileType,
          (importedFileType == ImportedFileType.SurveyedSurface || importedFileType == ImportedFileType.GeoTiff)
            ? surveyedUtc
            : null,
          dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, fileDescriptor,
          Guid.Parse(existing?.ImportedFileUid), existing.ImportedFileId,
          DataOceanRootFolderId, offset, dataOceanFileName);

        importedFile = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<UpdateImportedFileExecutor>(
              LoggerFactory, ConfigStore, ServiceExceptionHandler, CustomerUid, UserId, UserEmailAddress, customHeaders,
              productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction,
              tRexImportFileProxy: tRexImportFileProxy,
              projectRepo: ProjectRepo, dataOceanClient: DataOceanClient, authn: Authorization, schedulerProxy: schedulerProxy,
              cwsProjectClient: CwsProjectClient)
            .ProcessAsync(importedFileUpsertEvent)
        ) as ImportedFileDescriptorSingleResult;

        Logger.LogInformation(
          $"{nameof(UpsertFileInternal)}: Update completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      }

      await NotificationHubClient.Notify(new ProjectChangedNotification(projectUid));

      return importedFile;
    }
  }
}
