using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Common.Controller
{
  public class ImportedFileSynchronizerBase
  {
    protected IConfigurationStore ConfigStore;
    protected ILogger Log;
    protected ILoggerFactory Logger;
    protected string FileSpaceId;
    protected IRaptorProxy RaptorProxy;

    /// <summary>
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="raptorProxy"></param>
    public ImportedFileSynchronizerBase(IConfigurationStore configStore, ILoggerFactory logger,
      IRaptorProxy raptorProxy)
    {
      ConfigStore = configStore;
      Logger = logger;
      Log = logger.CreateLogger<ImportedFileSynchronizer>();
      RaptorProxy = raptorProxy;

      FileSpaceId = ConfigStore.GetValueString("TCCFILESPACEID");
      if (string.IsNullOrEmpty(FileSpaceId))
      {
        throw new InvalidOperationException(
          "ImportedFileSynchroniser unable to establish filespaceId");
      }
    }

    /// <summary>
    /// Notify raptor of new file
    ///     if it already knows about it, it will just update and re-notify raptor and return success.
    /// </summary>
    /// <returns></returns>
    protected async Task<bool> NotifyRaptorFileCreatedInCGenAsync(Guid projectUid,
      ImportedFileType importedFileType,
      Guid importedFileUid, string fileDescriptor, long legacyImportedFileId, DxfUnitsType dxfUnitsType)
    {
      var startUtc = DateTime.UtcNow;
      var isNotified = false;

      BaseDataResult notificationResult = null;
      // todo need to generate an application token for "3dPmScheduler" 
      IDictionary<string, string> customHeaders = null;
      try
      {
        notificationResult = await RaptorProxy
          .AddFile(projectUid, importedFileType, importedFileUid, fileDescriptor, legacyImportedFileId, dxfUnitsType, customHeaders)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"AddFile in RaptorServices failed with exception {e.Message}") },
          { "customHeaders", customHeaders},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid},
          { "fileDescriptor", fileDescriptor},
          { "legacyImportedFileId", legacyImportedFileId}
        };
        NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
      }
      Log.LogDebug(
        $"NotifyRaptorFileCreatedInCGen: projectUid:{projectUid} importedFileUid: {importedFileUid} FileDescriptor:{fileDescriptor}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult == null || notificationResult.Code != 0)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"AddFile in RaptorServices failed. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}") },
          { "customHeaders", customHeaders},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid},
          { "fileDescriptor", fileDescriptor},
          { "legacyImportedFileId", legacyImportedFileId}
        };
        NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
      }
      else
      {
        isNotified = true;
      }

      return isNotified;
    }

    /// <summary>
    /// Notify raptor of updated file
    ///     if it already knows about it, it will just update and re-notify raptor and return success.
    /// </summary>
    /// <returns></returns>
    protected async Task<bool> NotifyRaptorFileUpdatedInCGen(Guid projectUid, Guid importedFileUid)
    {
      var startUtc = DateTime.UtcNow;
      var isNotified = false;

      BaseDataResult notificationResult = null;
      // todo need to generate an application token for "3dPmScheduler" 
      IDictionary<string, string> customHeaders = null;
      try
      {
        notificationResult = await RaptorProxy
          .UpdateFiles(projectUid, new List<Guid>() {importedFileUid}, customHeaders)
          .ConfigureAwait(false);
        ;
      }
      catch (Exception e)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"UpdateFile in RaptorServices failed with exception {e.Message}") },
          { "customHeaders", customHeaders},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid}
        };
        NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
      }
      Log.LogDebug(
        $"NotifyRaptorFileUpdatedInCGen: projectUid:{projectUid} importedFileUid: {importedFileUid}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult == null || notificationResult.Code != 0)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"UpdateFile in RaptorServices failed. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}") },
          { "customHeaders", customHeaders},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid}
        };
        NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
      }
      else
      {
        isNotified = true;
      }

      return isNotified;
    }

    /// <summary>
    /// Notify raptor of new file
    ///     if it already knows about it, it will just update and re-notify raptor and return success.
    /// </summary>
    /// <returns></returns>
    protected async System.Threading.Tasks.Task<bool> NotifyRaptorFileDeletedInCGenAsync(Guid projectUid,
      ImportedFileType importedFileType,
      Guid importedFileUid, string fileDescriptor, long legacyImportedFileId)
    {
      var startUtc = DateTime.UtcNow;
      var isNotified = false;

      BaseDataResult notificationResult = null;
      // todo need to generate an application token for "3dPmScheduler" 
      IDictionary<string, string> customHeaders = null;
      try
      {
        notificationResult = await RaptorProxy
          .DeleteFile(projectUid, importedFileType, importedFileUid, fileDescriptor, legacyImportedFileId, customHeaders)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"DeleteFile in RaptorServices failed with exception {e.Message}") },
          { "customHeaders", customHeaders},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid},
          { "fileDescriptor", fileDescriptor},
          { "legacyImportedFileId", legacyImportedFileId}
        };
        NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
      }
      Log.LogDebug(
        $"NotifyRaptorFileDeletedInCGen: projectUid:{projectUid} importedFileUid: {importedFileUid} FileDescriptor:{fileDescriptor}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult == null || notificationResult.Code != 0)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"DeleteFile in RaptorServices failed. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}") },
          { "customHeaders", customHeaders},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid},
          { "fileDescriptor", fileDescriptor},
          { "legacyImportedFileId", legacyImportedFileId}
        };
        NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
      }
      else
      {
        isNotified = true;
      }

      return isNotified;
    }
  }
}
