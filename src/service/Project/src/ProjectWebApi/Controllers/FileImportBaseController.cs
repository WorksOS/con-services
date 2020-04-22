using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// FileImporter controller
  /// </summary>
  public class FileImportBaseController : BaseController<FileImportBaseController>
  {
    protected ITransferProxy persistantTransferProxy; 
    protected IFilterServiceProxy filterServiceProxy;
    protected ITRexImportFileProxy tRexImportFileProxy;

    protected string FileSpaceId;
    protected string DataOceanRootFolderId;
    protected bool UseTrexGatewayDesignImport;
    protected bool UseRaptorGatewayDesignImport;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IRequestFactory requestFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileImportBaseController"/> class.
    /// </summary>
    public FileImportBaseController(Func<TransferProxyType, ITransferProxy> persistantTransferProxy,
      IFilterServiceProxy filterServiceProxy, ITRexImportFileProxy tRexImportFileProxy, IRequestFactory requestFactory)
    {
      this.requestFactory = requestFactory;

      this.persistantTransferProxy = persistantTransferProxy(TransferProxyType.DesignImport);
      this.filterServiceProxy = filterServiceProxy;
      this.tRexImportFileProxy = tRexImportFileProxy;

      FileSpaceId = ConfigStore.GetValueString("TCCFILESPACEID");
      DataOceanRootFolderId = ConfigStore.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      UseTrexGatewayDesignImport = false;
      UseRaptorGatewayDesignImport = true;
      bool.TryParse(ConfigStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"),
        out UseTrexGatewayDesignImport);
      bool.TryParse(ConfigStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"),
        out UseRaptorGatewayDesignImport);
    }

    /// <summary>
    /// Validates a project identifier.
    /// </summary>
    protected async Task ValidateProjectId(string projectUid)
    {
      LogCustomerDetails("GetProject", projectUid);
      var project =
        (await ProjectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));
      if (project == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      Logger.LogInformation($"Project {JsonConvert.SerializeObject(project)} retrieved");
    }

    /// <summary>
    /// Notify raptor of an updated import file (used for activations, not file import).
    /// </summary>
    protected async Task NotifyRaptorUpdateFile(Guid projectUid, IEnumerable<Guid> updatedFileUids)
    {
      var notificationResult = await Productivity3dV2ProxyNotification.UpdateFiles(projectUid, updatedFileUids, Request.Headers.GetCustomHeaders());

      Logger.LogDebug(
        $"FileImport UpdateFiles in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 108, notificationResult.Code.ToString(), notificationResult.Message);
      }
    }

    /// <summary>
    /// Sets activated state for imported files.
    /// </summary>
    protected async Task<IEnumerable<Guid>> SetFileActivatedState(string projectUid, Dictionary<Guid, bool> fileUids)
    {
      Logger.LogDebug($"SetFileActivatedState: projectUid={projectUid}, {fileUids.Keys.Count} files with changed state");

      var deactivatedFileList = await ImportedFileRequestDatabaseHelper.GetImportedFileProjectSettings(projectUid, userId, ProjectRepo).ConfigureAwait(false) ?? new List<ActivatedFileDescriptor>();
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
        requestFactory.Create<ProjectSettingsRequestHelper>(r => r
            .CustomerUid(customerUid))
          .CreateProjectSettingsRequest(projectUid, JsonConvert.SerializeObject(deactivatedFileList), ProjectSettingsType.ImportedFiles);
      projectSettingsRequest.Validate();

      _ = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpsertProjectSettingsExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, headers: customHeaders,
            productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction, projectRepo: ProjectRepo)
          .ProcessAsync(projectSettingsRequest)
      ) as ProjectSettingsResult;

      var changedUids = fileUids.Keys.Except(missingUids);
      Logger.LogDebug($"SetFileActivatedState: {changedUids.Count()} changedUids");

      return changedUids;
    }

    protected bool IsDesignFileType(ImportedFileType importedFileType)
    {
      return importedFileType == ImportedFileType.DesignSurface ||
             importedFileType == ImportedFileType.SurveyedSurface ||
             importedFileType == ImportedFileType.Alignment;
      //Don't save reference surface to s3 (The original design file will have been saved).
    }
  }
}
