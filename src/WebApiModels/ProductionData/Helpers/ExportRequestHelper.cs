using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.UserPreferences;
using BoundingExtents;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Extensions;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Filter = VSS.Productivity3D.Common.Models.Filter;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ExportRequestHelper : DataRequestBase, IExportRequestHandler
  {
    private const string ALL_MACHINES = "All";
    private IASNodeClient RaptorClient;
    private UserPreferenceData userPreferences;
    private ProjectDescriptor projectDescriptor;

    public ExportRequestHelper()
    { }

    public ExportRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }
    public ExportRequestHelper SetRaptorClient(IASNodeClient raptorClient)
    {
      RaptorClient = raptorClient;
      return this;
    }

    public ExportRequestHelper SetPreferencesProxy(IPreferenceProxy preferenceProxy)
    {
      userPreferences = preferenceProxy.GetUserPreferences(Headers).Result;
      return this;
    }


    public ExportRequestHelper SetProjectDescriptor(ProjectDescriptor projectDescriptor)
    {
      this.projectDescriptor = projectDescriptor;
      return this;
    }

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class and populate it with data needed for a design profile.   
    /// </summary>
    /// <returns>An instance of the ProfileProductionDataRequest class.</returns>
    public async Task<ExportReport> CreateExportRequest(Guid projectUid,
      DateTime? startUtc,
      DateTime? endUtc,
      CoordTypes coordType,
      ExportTypes exportType,
      string fileName,
      bool restrictSize,
      bool rawData,
      OutputTypes outputType,
      string machineNames,
      double tolerance = 0.0)
    {
      //var projectId = (User as RaptorPrincipal).GetProjectId(projectUid);

//      var userPref = await userPreferences.GetUserPreferences(Headers);

      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Pass count settings required for detailed pass count report"));
      }
  //    var projectSettings = await GetProjectSettings(projectUid);
      LiftBuildSettings liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      var excludedIds = await GetExcludedSurveyedSurfaceIds(FileListProxy, projectUid);

      // Filter filter = settingsManager.CompactionFilter(startUtc, endUtc, null, null, null, null, this.GetMachines(assetId, machineName, isJohnDoe), null);
      Filter filter = SettingsManager.CompactionFilter(null, null, null, null, null, null, null, excludedIds);

      T3DBoundingWorldExtent projectExtents = new T3DBoundingWorldExtent();
      TMachine[] machineList = null;

      if (exportType == ExportTypes.kSurfaceExport)
      {

        RaptorClient.GetDataModelExtents(ProjectId,
          RaptorConverters.convertSurveyedSurfaceExlusionList(ExcludedIds), out projectExtents);
      }
      else
      {
        TMachineDetail[] machineDetails = RaptorClient.GetMachineIDs(ProjectId);

        if (machineDetails != null)
        {
          //machineDetails = machineDetails.GroupBy(x => x.Name).Select(y => y.Last()).ToArray();

          if (machineNames != null)
          {
            if (machineNames != ALL_MACHINES)
            {
              var machineNamesArray = machineNames.Split(',');
              machineDetails = machineDetails.Where(machineDetail => machineNamesArray.Contains(machineDetail.Name)).ToArray();
            }
          }

          machineList = machineDetails.Select(m => new TMachine { AssetID = m.ID, MachineName = m.Name, SerialNo = "" }).ToArray();
        }
      }

      // Set User Preferences' time zone to the project's one and retriev ...
//      var projectDescriptor = (User as RaptorPrincipal).GetProject(projectUid);
      userPreferences.Timezone = projectDescriptor.projectTimeZone;

      if (!string.IsNullOrEmpty(fileName))
      {
        // Strip invalid characters from the file name...
        fileName = StripInvalidCharacters(fileName);
      }

      return ExportReport.CreateExportReportRequest(
        ProjectId,
        liftSettings,
        filter,
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
        false,
        fileName,
        exportType,
        ConvertUserPreferences(userPreferences));
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

    /// <summary>
    /// Converts a set user preferences in the format understood by the Raptor for.
    /// It is solely used by production data export WebAPIs.
    /// </summary>
    /// <param name="userPref">The set of user preferences.</param>
    /// <returns>The set of user preferences in Raptor's format</returns>
    private static TASNodeUserPreferences ConvertUserPreferences(UserPreferenceData userPref)
    {
      TimeZoneInfo projecTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userPref.Timezone);
      double projectTimeZoneOffset = projecTimeZone.GetUtcOffset(DateTime.Now).TotalHours;

      return ASNode.UserPreferences.__Global.Construct_TASNodeUserPreferences(
        userPref.Timezone,
        Preferences.DefaultDateSeparator,
        Preferences.DefaultTimeSeparator,
        userPref.ThousandsSeparator,
        userPref.DecimalSeparator,
        projectTimeZoneOffset,
        Array.IndexOf(LanguageLocales.LanguageLocaleStrings, userPref.Language),
        (int)UnitsTypeEnum.Metric,
        Preferences.DefaultDateTimeFormat,
        Preferences.DefaultNumberFormat,
        Preferences.DefaultTemperatureUnit,
        Preferences.DefaultAssetLabelTypeId);
    }

    protected async Task<List<long>> GetExcludedSurveyedSurfaceIds(IFileListProxy fileListProxy, Guid projectUid)
    {
      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), Headers);
      if (fileList == null || fileList.Count == 0)
      {
        return null;
      }

      var results = fileList
        .Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface && !f.IsActivated)
        .Select(f => f.LegacyFileId).ToList();

      return results;
    }

  }
}