using System;
using System.Linq;
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.UserPreferences;
using BoundingExtents;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ExportRequestHelper : DataRequestBase, IExportRequestHandler
  {
    private IASNodeClient raptorClient;
    private UserPreferenceData userPreferences;
    private ProjectData projectDescriptor;

    /// <summary>
    /// Parameterless constructor is required to support factory create function in <see cref="WebApi"/> project.
    /// </summary>
    public ExportRequestHelper()
    { }

    public ExportRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    public ExportRequestHelper SetRaptorClient(IASNodeClient raptorClient)
    {
      this.raptorClient = raptorClient;
      return this;
    }

    public ExportRequestHelper SetUserPreferences(UserPreferenceData userPrefs)
    {
      userPreferences = userPrefs;
      return this;
    }

    public ExportRequestHelper SetProjectDescriptor(ProjectData projectDescriptor)
    {
      this.projectDescriptor = projectDescriptor;
      return this;
    }

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class and populate it with data needed for a design profile.   
    /// </summary>
    public ExportReport CreateExportRequest(
      DateTime? startUtc,
      DateTime? endUtc,
      CoordType coordType,
      ExportTypes exportType,
      string fileName,
      bool restrictSize,
      bool rawData,
      OutputTypes outputType,
      string machineNames,
      double tolerance = 0.0)
    {
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      T3DBoundingWorldExtent projectExtents = new T3DBoundingWorldExtent();
      TMachine[] machineList = null;

      if (exportType == ExportTypes.SurfaceExport)
      {
        raptorClient.GetDataModelExtents(ProjectId,
          RaptorConverters.convertSurveyedSurfaceExlusionList(Filter?.SurveyedSurfaceExclusionList), out projectExtents);
      }
      else
      {
        TMachineDetail[] machineDetails = raptorClient.GetMachineIDs(ProjectId);

        if (machineDetails != null)
        {
          if (!string.IsNullOrEmpty(machineNames) && machineNames != "All")
          {
            var machineNamesArray = machineNames.Split(',');
            machineDetails = machineDetails.Where(machineDetail => machineNamesArray.Contains(machineDetail.Name)).ToArray();
          }

          machineList = machineDetails.Select(m => new TMachine { AssetID = m.ID, MachineName = m.Name, SerialNo = "" }).ToArray();
        }
      }

      if (!string.IsNullOrEmpty(fileName))
      {
        fileName = StripInvalidCharacters(fileName);
      }

      return ExportReport.CreateExportReportRequest(
        ProjectId,
        liftSettings,
        Filter,
        -1,
        null,
        false,
        null,
        coordType,
        startUtc ?? DateTime.MinValue,
        endUtc ?? DateTime.MinValue,
        true,
        tolerance,
        false,
        restrictSize,
        rawData,
        projectExtents,
        false,
        outputType,
        machineList,
        exportType == ExportTypes.SurfaceExport,
        fileName,
        exportType,
        ConvertUserPreferences(userPreferences, projectDescriptor.ProjectTimeZone));
    }

    private static string StripInvalidCharacters(string str)
    {
      // Remove all invalid characters except of the underscore...
      str = System.Text.RegularExpressions.Regex.Replace(str, @"[^A-Za-z0-9\s-\w\/_]", "");

      // Convert multiple spaces into one space...
      str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();

      // Replace spaces with undescore characters...
      str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "_");

      return str;
    }

    // TODO (Aaron) move to RaptopHelper
    /// <summary>
    /// Converts a set user preferences in the format understood by Raptor.
    /// It is solely used by production data export WebAPIs.
    /// </summary>
    /// <param name="userPref">The set of user preferences.</param>
    /// <param name="projectTimezone">The project time zone.</param>
    /// <returns>The set of user preferences in Raptor's format</returns>
    public static TASNodeUserPreferences ConvertUserPreferences(UserPreferenceData userPref, string projectTimezone)
    {
      var timezone = projectTimezone ?? userPref.Timezone;
      TimeZoneInfo projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
      double projectTimeZoneOffset = projectTimeZone?.GetUtcOffset(DateTime.Now).TotalHours ?? 0;

      var languageIndex = Array.FindIndex(LanguageLocales.LanguageLocaleStrings, s => s.Equals(userPref.Language, StringComparison.OrdinalIgnoreCase));
      
      if (languageIndex == -1)
      {
        languageIndex = (int)LanguageEnum.enUS;
      }

      return ASNode.UserPreferences.__Global.Construct_TASNodeUserPreferences(
        timezone,
        Preferences.DefaultDateSeparator,
        Preferences.DefaultTimeSeparator,
        //Hardwire number format as "xxx,xxx.xx" or it causes problems with the CSV file as comma is the column separator.
        //To respect user preferences requires Raptor to enclose formatted numbers in quotes.
        //This bug is present in CG since it uses user preferences separators.
        Preferences.DefaultThousandsSeparator,
        Preferences.DefaultDecimalSeparator,
        projectTimeZoneOffset,
        languageIndex,
        (int)userPref.Units.UnitsType(),
        Preferences.DefaultDateTimeFormat,
        Preferences.DefaultNumberFormat,
        (int)userPref.TemperatureUnit.TemperatureUnitType(),
        Preferences.DefaultAssetLabelTypeId);
    }
  }
}
