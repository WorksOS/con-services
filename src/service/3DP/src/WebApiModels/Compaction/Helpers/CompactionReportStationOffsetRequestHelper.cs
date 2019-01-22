using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;


namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// The request representation for getting production data from Raptor for a station and offset report.
  /// </summary>
  /// 
  public class CompactionReportStationOffsetRequestHelper : DataRequestBase, ICompactionReportStationOffsetRequestHelper
  {
    public string projectTimezone { get; set; }
  
    /// <summary>
    /// Parameterless constructor is required to support factory create function in <see cref="VSS.Productivity3D.WebApi"/> project.
    /// </summary>
    public CompactionReportStationOffsetRequestHelper()
    { }

    public CompactionReportStationOffsetRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    public CompactionReportStationOffsetRequest CreateRequest(bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill, DesignDescriptor cutFillDesignDescriptor, DesignDescriptor alignmentDescriptor, double crossSectionInterval, double startStation, double endStation, double[] offsets, UserPreferenceData userPreferences, string projectTimzone)
    {
      var liftBuildSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      return CompactionReportStationOffsetRequest.CreateRequest(
        ProjectId,
        ProjectUid,
        Filter,
        Filter != null ? Filter.Id ?? -1 : -1,
        liftBuildSettings,
        reportElevation,
        reportCmv,
        reportMdp,
        reportPassCount,
        reportTemperature,
        reportCutFill,
        cutFillDesignDescriptor,
        alignmentDescriptor,
        crossSectionInterval,
        startStation,
        endStation,
        offsets,
        userPreferences,
        projectTimzone);
    }
  }
}
