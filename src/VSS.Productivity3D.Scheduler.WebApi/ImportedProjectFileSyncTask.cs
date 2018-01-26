using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Controller;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// ImportedProjectFileSyncTask syncs the importedFiles table between 2 databases 
  ///   1) MySql Project.ImportedFiles
  ///   2) MSSql NH_OP.ImportedFiles 
  /// </summary>
  public abstract class ImportedProjectFileSyncTask
  {
    protected readonly IConfigurationStore _configStore;
    private readonly ILoggerFactory _logger;
    protected readonly ILogger _log;
    private readonly IRaptorProxy _raptorProxy;
    private readonly ITPaasProxy _tPaasProxy;
    private readonly IImportedFileProxy _impFileProxy;
    private readonly IFileRepository _fileRepo;
    protected static int DefaultTaskIntervalDefaultMinutes { get; } = 4;

    /// <summary>
    /// Initializes the ImportedProjectFileSyncTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="tPaasProxy"></param>
    /// <param name="impFileProxy"></param>
    /// <param name="fileRepo"></param>
    public ImportedProjectFileSyncTask(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy,
      ITPaasProxy tPaasProxy, IImportedFileProxy impFileProxy, IFileRepository fileRepo)
    {
      _configStore = configStore;
      _logger = logger;
      _log = logger.CreateLogger<ImportedProjectFileSyncTask>();
      _raptorProxy = raptorProxy;
      _tPaasProxy = tPaasProxy;
      _impFileProxy = impFileProxy;
      _fileRepo = fileRepo;
    }

    /// <summary>
    /// bi-sync between 2 databases, 1 table in each
    /// </summary>
    protected void ImportedFilesSyncTask(bool processSurveyedSurfaceType)
    {
      _log.LogDebug($"ImportedFilesSyncTask: ProcessSurveyedSurfaceType={processSurveyedSurfaceType}");

      var startUtc = DateTime.UtcNow;
      _log.LogDebug($"ImportedFilesSyncTask()  beginning. startUtc: {startUtc}");

      var sync = new ImportedFileSynchronizer(_configStore, _logger, _raptorProxy, _tPaasProxy, _impFileProxy, _fileRepo, processSurveyedSurfaceType);
      sync.SyncTables().Wait();

      var newRelicAttributes = new Dictionary<string, object> {
        { "message", "Task completed." }
      };
      NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log, newRelicAttributes);
      _log.LogDebug($"ImportedFilesSyncTask()  ended. endUtc: {DateTime.UtcNow}");

    }
  }
}
