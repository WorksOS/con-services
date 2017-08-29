using ASNode.ExportProductionDataCSV.RPC;
using BoundingExtents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionExportController : BaseController
  {
    private const string ALL_MACHINES = "All";
    private const double SURFACE_EXPORT_TOLLERANCE = 0.05;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// For retrieving user preferences
    /// </summary>
    private readonly IPreferenceProxy prefProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient">The raptor client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configStore">Configuration store</param>/// 
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    public CompactionExportController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy, IPreferenceProxy prefProxy) : 
      base (logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.log = logger.CreateLogger<CompactionExportController>();
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.prefProxy = prefProxy;
    }

    /// <summary>
    /// Gets an export of 3D project data in .TTM file format report.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="tolerance">Controls triangulation density in the output .TTM file.</param>
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/export/surface")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportSurface(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] double? tolerance
    )
    {
      log.LogInformation("GetExportReportSurface: " + Request.QueryString);

      tolerance = tolerance ?? SURFACE_EXPORT_TOLLERANCE;

      var exportRequest = await GetExportReportRequest(
        projectUid,
        null, //startUtc,
        null, //endUtc,
        CoordTypes.ptNORTHEAST,
        ExportTypes.kSurfaceExport,
        fileName,
        false,
        false,
        OutputTypes.etVedaAllPasses,
        "",
        tolerance.Value);

      exportRequest.Validate();

      return RequestExecutorContainerFactory.Build<ExportReportExecutor>(logger, raptorClient, null, configStore).Process(exportRequest) as ExportResult;
    }

    /// <summary>
    /// Gets an export of production data in cell grid format report for import to VETA.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="machineNames">Comma-separated list of machine names.</param>
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/export/veta")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportVeta(
      [FromQuery] Guid projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] string fileName,
      [FromQuery] string machineNames)
    {
      log.LogInformation("GetExportReportVeta: " + Request.QueryString);

      var exportRequest = await GetExportReportRequest(
        projectUid,
        startUtc,
        endUtc,
        CoordTypes.ptNORTHEAST,
        ExportTypes.kVedaExport,
        fileName,
        false,
        true,
        OutputTypes.etVedaAllPasses,
        machineNames);

      exportRequest.Validate();

      return RequestExecutorContainerFactory.Build<ExportReportExecutor>(logger, raptorClient, null, configStore).Process(exportRequest) as ExportResult;
    }

    /// <summary>
    /// Gets an export of production data in cell grid format report.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC.</param>
    /// <param name="coordType">Either Northing/Easting or Latitude/Longitude.</param>
    /// <param name="outputType">Either all passes/last for pass machine passes export or all passes/final pass for export for VETA</param>
    /// <param name="restrictOutput">Output .CSV file is restricted to 65535 rows if it is true.</param>
    /// <param name="rawDataOutput">Column headers in an output .CSV file's are in the dBase format.</param>
    /// <param name="fileName">Output file name.</param>
    /// <returns>An instance of the ExportResult class.</returns>
    [ProjectUidVerifier]
    [Route("api/v2/export/machinepasses")]
    [HttpGet]
    public async Task<ExportResult> GetExportReportMachinePasses(
      [FromQuery] Guid projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] int coordType,
      [FromQuery] int outputType,
      [FromQuery] bool restrictOutput,
      [FromQuery] bool rawDataOutput,
      [FromQuery] string fileName)
    {
      log.LogInformation("GetExportReportMachinePasses: " + Request.QueryString);

      var exportRequest = await GetExportReportRequest(
        projectUid,
        startUtc,
        endUtc,
        (CoordTypes)coordType,
        ExportTypes.kPassCountExport,
        fileName,
        restrictOutput,
        rawDataOutput,
        (OutputTypes)outputType,
        "");

      exportRequest.Validate();

      return RequestExecutorContainerFactory.Build<ExportReportExecutor>(logger, raptorClient, null, configStore).Process(exportRequest) as ExportResult;
    }

    /// <summary>
    /// Creates an instance of the CMVRequest class and populate it with data.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC.</param>
    /// <param name="coordType">Either Northing/Easting or Latitude/Longitude.</param>
    /// <param name="exportType">Export production data a surface .TTM file, machine passes export or machine passes export for an import to VETA.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="restrictSize">Output .CSV file is restricted to 65535 rows if it is true.</param>
    /// <param name="rawData">Column headers in an output .CSV file's are in the dBase format.</param>
    /// <param name="outputType">Either all passes/last for pass machine passes export or all passes/final pass for export for VETA</param>
    /// <param name="machineNames">Comma-separated list of machine names.</param>
    /// <param name="tolerance">It is used in export to a surface .TTM file.</param>
    /// <returns>An instance of the ExportReport class.</returns>
    private async Task<ExportReport> GetExportReportRequest(
      Guid projectUid,
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
      var projectId = (User as RaptorPrincipal).GetProjectId(projectUid);

      var headers = Request.Headers.GetCustomHeaders();
      var userPref = await prefProxy.GetUserPreferences(headers);

      if (userPref == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Pass count settings required for detailed pass count report"));
      }
      var projectSettings = await GetProjectSettings(projectUid);
      LiftBuildSettings liftSettings = settingsManager.CompactionLiftBuildSettings(projectSettings);

      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid);

      // Filter filter = settingsManager.CompactionFilter(startUtc, endUtc, null, null, null, null, this.GetMachines(assetId, machineName, isJohnDoe), null);
      Filter filter = settingsManager.CompactionFilter(null, null, null, null, null, null, null, excludedIds);

      T3DBoundingWorldExtent projectExtents = new T3DBoundingWorldExtent();
      TMachine[] machineList = null;

      if (exportType == ExportTypes.kSurfaceExport)
      {
        raptorClient.GetDataModelExtents(projectId,
          RaptorConverters.convertSurveyedSurfaceExlusionList(excludedIds), out projectExtents);
      }
      else
      {
        TMachineDetail[] machineDetails = raptorClient.GetMachineIDs(projectId);

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

          machineList = machineDetails.Select(m => new TMachine() { AssetID = m.ID, MachineName = m.Name, SerialNo = "" }).ToArray();
        }
      }

      // Set User Preferences' time zone to the project's one and retriev ...
      var projectDescriptor = (User as RaptorPrincipal).GetProject(projectUid);
      userPref.Timezone = projectDescriptor.projectTimeZone;

      if (!String.IsNullOrEmpty(fileName))
      {
        // Strip invalid characters from the file name...
        fileName = StripInvalidCharacters(fileName);
      }

      return ExportReport.CreateExportReportRequest(
        projectId,
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
        this.convertUserPreferences(userPref));
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
  }
}