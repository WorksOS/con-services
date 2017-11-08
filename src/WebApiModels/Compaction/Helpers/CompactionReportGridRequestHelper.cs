using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// The request representation for getting production data from Raptor for a grid report.
  /// </summary>
  /// 
  public class CompactionReportGridRequestHelper : DataRequestBase, ICompactionReportGridRequestHelper
  {
    private IASNodeClient raptorClient;
    //private UserPreferenceData userPreferences;

    /// <summary>
    /// Parameterless constructor is required to support factory create function in <see cref="VSS.Productivity3D.WebApi"/> project.
    /// </summary>
    public CompactionReportGridRequestHelper()
    { }

    public CompactionReportGridRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    public CompactionReportGridRequestHelper SetRaptorClient(IASNodeClient raptorClient)
    {
      this.raptorClient = raptorClient;
      return this;
    }

//    public ExportRequestHelper SetUserPreferences(UserPreferenceData userPrefs)
//    {
//      userPreferences = userPrefs;
//      return this;
//    }


    public async Task<CompactionReportGridRequest> CreateCompactionReportGridRequest(
      bool reportElevation,
      bool reportCMV,
      bool reportMDP,
      bool reportPassCount,
      bool reportTemperature,
      bool reportCutFill,
      DesignDescriptor designFile,
      double? gridInerval,
      GridReportOption gridReportOption,
      double startNorthing,
      double startEasting,
      double endNorthing,
      double endEasting,
      double azimuth)
    {
      var liftBuildSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      return CompactionReportGridRequest.CreateCompactionReportGridRequest(
        ProjectId,
        Filter,
        Filter != null ? Filter.Id ?? -1 : -1,
        liftBuildSettings,
        reportElevation,
        reportCMV,
        reportMDP,
        reportPassCount,
        reportTemperature,
        reportCutFill,
        designFile,
        gridInerval,
        gridReportOption,
        startNorthing,
        startEasting,
        endNorthing,
        endEasting,
        azimuth);
    }
  }
}
