using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.WebApi;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Scheduler.WebAPI
{
  /// <summary>
  /// Task for processing and sync'ing imported surveyed surface files.
  /// </summary>
  public class SurveyedSurfaceFileSyncTask : ImportedProjectFileSyncTask
  {
    public SurveyedSurfaceFileSyncTask(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy,
      ITPaasProxy tPaasProxy, IImportedFileProxy impFileProxy, IFileRepository fileRepo) :
      base (configStore, logger, raptorProxy, tPaasProxy, impFileProxy, fileRepo)
    {
      ProcessSurveyedSurfaceType = true;
    }
  }
}
