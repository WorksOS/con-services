using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Compaction.Models;
using VSS.Raptor.Service.WebApiModels.Compaction.Models.Palettes;
using VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Executors;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;
using ColorValue = VSS.Raptor.Service.WebApiModels.Compaction.Models.Palettes.ColorValue;

namespace VSS.Raptor.Service.WebApi.Compaction.Controllers
{
  public class CompactionController : Controller
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Used to get list of projects for customer
    /// </summary>
    private readonly IAuthenticatedProjectsStore authProjectsStore;

    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;


    /// <summary>
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    public CompactionController(IASNodeClient raptorClient, ILoggerFactory logger,
      IAuthenticatedProjectsStore authProjectsStore)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionController>();
      this.authProjectsStore = authProjectsStore;
    }

    /// <summary>
    /// Creates an instance of the CMVRequest class and populate it with data.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectUid"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <param name="cmvMin"></param>
    /// <param name="cmvMax"></param>
    /// <returns>An instance of the CMVRequest class.</returns>
    private CMVRequest GetCMVRequest(long? projectId, Guid? projectUid, DateTime? startUtc, DateTime? endUtc, short cmvTarget = 0, short cmvMin = 0, short cmvMax = 0)
    {
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      CMVSettings cmvSettings = CMVSettings.CreateCMVSettings(cmvTarget, cmvMax, 120, cmvMin, 80, false);
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
            ? null
            : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }

      return CMVRequest.CreateCMVRequest(projectId.Value, null, cmvSettings, liftSettings, filter, -1, null, null, null);
    }

    /// <summary>
    /// Creates an instance of the PassCounts class and populate it with data.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectUid"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <returns>An instance of the PassCounts class.</returns>
    private PassCounts GetPassCountRequest(long? projectId, Guid? projectUid, DateTime? startUtc, DateTime? endUtc)
    {
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }

      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }

      return PassCounts.CreatePassCountsRequest(projectId.Value, null, null, liftSettings, filter, -1, null, null, null);
    }

    #region Summary Data for Widgets
    /// <summary>
    /// Get CMV summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>CMV summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/summary")]
    [HttpGet]
    public CompactionCmvSummaryResult GetCmvSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetCmvSummary: " + Request.QueryString);

      CMVRequest request = GetCMVRequest(projectId, projectUid, startUtc, endUtc);
      request.Validate();

      try
      {
        var result =
          RequestExecutorContainer.Build<SummaryCMVExecutor>(logger, raptorClient, null).Process(request) as
            CMVSummaryResult;
        var returnResult = CompactionCmvSummaryResult.CreateCmvSummaryResult(result, request.cmvSettings);
        log.LogInformation("GetCmvSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
      finally
      {
        log.LogInformation("GetCmvSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get MDP summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>MDP summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/mdp/summary")]
    [HttpGet]
    public CompactionMdpSummaryResult GetMdpSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetMdpSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      MDPSettings mdpSettings = MDPSettings.CreateMDPSettings(0, 0, 120, 0, 80, false);
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      MDPRequest request = MDPRequest.CreateMDPRequest(projectId.Value, null, mdpSettings, liftSettings, filter, -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummaryMDPExecutor>(logger, raptorClient, null).Process(request) as MDPSummaryResult;
        var returnResult = CompactionMdpSummaryResult.CreateMdpSummaryResult(result, mdpSettings);
        log.LogInformation("GetMdpSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
      finally
      {
        log.LogInformation("GetMdpSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>Pass count summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/passcounts/summary")]
    [HttpGet]
    public CompactionPassCountSummaryResult GetPassCountSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetPassCountSummary: " + Request.QueryString);

      PassCounts request = GetPassCountRequest(projectId, projectUid, startUtc, endUtc);
      request.Validate();

      try
      {
        var result = RequestExecutorContainer.Build<SummaryPassCountsExecutor>(logger, raptorClient, null).Process(request) as PassCountSummaryResult;
        var returnResult = CompactionPassCountSummaryResult.CreatePassCountSummaryResult(result);
        log.LogInformation("GetPassCountSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
      finally
      {
        log.LogInformation("GetPassCountSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Temperature summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>Temperature summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/temperature/summary")]
    [HttpGet]
    public CompactionTemperatureSummaryResult GetTemperatureSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetTemperatureSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      TemperatureSettings temperatureSettings = TemperatureSettings.CreateTemperatureSettings(0, 175, 65, false);
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      TemperatureRequest request = TemperatureRequest.CreateTemperatureRequest(projectId.Value, null, temperatureSettings, liftSettings, filter, -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummaryTemperatureExecutor>(logger, raptorClient, null).Process(request) as TemperatureSummaryResult;
        var returnResult = CompactionTemperatureSummaryResult.CreateTemperatureSummaryResult(result);
        log.LogInformation("GetTemperatureSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
      finally
      {
        log.LogInformation("GetTemperatureSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Speed summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>Speed summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/speed/summary")]
    [HttpGet]
    public CompactionSpeedSummaryResult GetSpeedSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetSpeedSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>(
          "{'liftDetectionType': '4', 'machineSpeedTarget': { 'MinTargetMachineSpeed': '333', 'MaxTargetMachineSpeed': '417'}}");
        //liftDetectionType 4 = None, speeds are cm/sec (12 - 15 km/hr)
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      SummarySpeedRequest request = SummarySpeedRequest.CreateSummarySpeedRequestt(projectId.Value, null, liftSettings, filter, -1);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummarySpeedExecutor>(logger, raptorClient, null).Process(request) as SummarySpeedResult;
        var returnResult = CompactionSpeedSummaryResult.CreateSpeedSummaryResult(result, liftSettings.machineSpeedTarget);
        log.LogInformation("GetSpeedSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
      finally
      {
        log.LogInformation("GetSpeedSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get CMV % change from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>CMV % change</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/percentchange")]
    [HttpGet]
    public CompactionCmvPercentChangeResult GetCmvPercentChange([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetCmvPercentChange: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      double[] cmvChangeSummarySettings = new double[] { 5, 20, 50, NO_CCV };
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      CMVChangeSummaryRequest request = CMVChangeSummaryRequest.CreateCMVChangeSummaryRequest(projectId.Value, null, liftSettings, filter, -1,
        cmvChangeSummarySettings);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<CMVChangeSummaryExecutor>(logger, raptorClient, null).Process(request) as CMVChangeSummaryResult;
        var returnResult = CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(result, cmvChangeSummarySettings);
        log.LogInformation("GetCmvPercentChange result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }

        throw;
      }
      finally
      {
        log.LogInformation("GetCmvPercentChange returned: " + Response.StatusCode);
      }
    }

        #endregion

    #region Detailed Data for the map
    /// <summary>
    /// Get CMV details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>CMV details</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/details")]
    [HttpGet]
    public CompactionCmvDetailedResult GetCmvDetails([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
        [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetCmvDetails: " + Request.QueryString);

      CMVRequest request = GetCMVRequest(projectId, projectUid, startUtc, endUtc, 70, 20, 100);
      request.Validate();

      try
      {
        var result = RequestExecutorContainer.Build<DetailedCMVExecutor>(logger, raptorClient, null).Process(request) as CMVDetailedResult;
        var returnResult = CompactionCmvDetailedResult.CreateCmvDetailedResult(result);

        log.LogInformation("GetCmvDetails result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException se)
      {
          //Change FailedToGetResults to 204
          if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
              se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
          {
              se.Response.StatusCode = HttpStatusCode.NoContent;
          }
          throw;
      }
      finally
      {
          log.LogInformation("GetCmvDetails returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>Pass count details</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/passcounts/details")]
    [HttpGet]
    public CompactionPassCountDetailedResult GetPassCountDetails([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetPassCountDetails: " + Request.QueryString);

      PassCounts request = GetPassCountRequest(projectId, projectUid, startUtc, endUtc);
      request.Validate();

      try
      {
        var result = RequestExecutorContainer.Build<DetailedPassCountExecutor>(logger, raptorClient, null).Process(request) as PassCountDetailedResult;
        var returnResult = CompactionPassCountDetailedResult.CreatePassCountDetailedResult(result);
        log.LogInformation("GetPassCountDetails result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
      finally
      {
        log.LogInformation("GetPassCountDetails returned: " + Response.StatusCode);
      }
    }

    #endregion

    #region Palettes

    /// <summary>
    /// Get color palettes.
    /// </summary>
    /// <returns>Color palettes for all display types</returns>
    [Route("api/v2/compaction/colorpalettes")]
    [HttpGet]
    public CompactionColorPalettesResult GetColorPalettes()
    {
      List<DisplayMode> modes = new List<DisplayMode>
      {
        DisplayMode.Height, DisplayMode.CCV, DisplayMode.PassCount, DisplayMode.PassCountSummary, DisplayMode.CutFill, DisplayMode.TemperatureSummary, DisplayMode.CCVPercentSummary, DisplayMode.MDPPercentSummary, DisplayMode.TargetSpeedSummary, DisplayMode.CMVChange
      };

      DetailPalette elevationPalette = null;
      DetailPalette cmvDetailPalette = null;
      DetailPalette passCountDetailPalette = null;
      SummaryPalette passCountSummaryPalette = null;
      DetailPalette cutFillPalette = null;
      SummaryPalette temperatureSummaryPalette = null;
      SummaryPalette cmvSummaryPalette = null;
      SummaryPalette mdpSummaryPalette = null;
      DetailPalette cmvPercentChangePalette = null;
      SummaryPalette speedSummaryPalette = null;

      //This is temporary until temperature details implemented in Raptor.
      DetailPalette temperatureDetailPalette = DetailPalette.CreateDetailPalette(
        new List<ColorValue>
        {
          ColorValue.CreateColorValue(0x2D5783, 70),
          ColorValue.CreateColorValue(0x439BDC, 80),
          ColorValue.CreateColorValue(0xBEDFF1, 90),
          ColorValue.CreateColorValue(0xDCEEC7, 100),
          ColorValue.CreateColorValue(0x9DCE67, 110),
          ColorValue.CreateColorValue(0x6BA03E, 120),
          ColorValue.CreateColorValue(0x3A6B25, 130),
          ColorValue.CreateColorValue(0xF6CED3, 140),
          ColorValue.CreateColorValue(0xD57A7C, 150),
          ColorValue.CreateColorValue(0xC13037, 160)
        },
        Colors.Red, Colors.Black);


      foreach (var mode in modes)
      {
        List<ColorValue> colorValues;
        var raptorPalette = RaptorConverters.convertColorPalettes(null, mode).Transitions;
        switch (mode)
        {
          case DisplayMode.Height:
            colorValues = new List<ColorValue>();
            for (int i = 1; i < raptorPalette.Length - 1; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
            }
            elevationPalette = DetailPalette.CreateDetailPalette(colorValues, raptorPalette[raptorPalette.Length-1].Colour, raptorPalette[0].Colour);
            break;
          case DisplayMode.CCV:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < raptorPalette.Length; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
            }
            //above hardcoded in Raptor to RGB 128,128,128. No below required as minimum is 0.
            cmvDetailPalette = DetailPalette.CreateDetailPalette(colorValues, Colors.Gray, null);
            break;
          case DisplayMode.PassCount:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < raptorPalette.Length - 1; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
            }
            passCountDetailPalette = DetailPalette.CreateDetailPalette(colorValues, raptorPalette[raptorPalette.Length-1].Colour, null);
            break;
          case DisplayMode.PassCountSummary:
            passCountSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[2].Colour, raptorPalette[1].Colour, raptorPalette[0].Colour);
            break;
          case DisplayMode.CutFill:
            colorValues = new List<ColorValue>();
            for (int i = raptorPalette.Length - 1; i >= 0; i--)
            {
              colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
            }
            cutFillPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
            break;
          case DisplayMode.TemperatureSummary:
            temperatureSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[2].Colour, raptorPalette[1].Colour, raptorPalette[0].Colour);
            break;
          case DisplayMode.CCVPercentSummary:
            cmvSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[3].Colour, raptorPalette[0].Colour, raptorPalette[2].Colour);
            break;
          case DisplayMode.MDPPercentSummary:
            mdpSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[3].Colour, raptorPalette[0].Colour, raptorPalette[2].Colour);
            break;
          case DisplayMode.TargetSpeedSummary:
            speedSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[2].Colour, raptorPalette[1].Colour, raptorPalette[0].Colour);
            break;
          case DisplayMode.CMVChange:
            colorValues = new List<ColorValue>();
            for (int i = 1; i < raptorPalette.Length - 1; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
            }
            cmvPercentChangePalette = DetailPalette.CreateDetailPalette(colorValues, raptorPalette[raptorPalette.Length - 1].Colour, raptorPalette[0].Colour);
            break;
        }

      }
      return CompactionColorPalettesResult.CreateCompactionColorPalettesResult(
        elevationPalette, cmvDetailPalette, passCountDetailPalette, passCountSummaryPalette, cutFillPalette, temperatureSummaryPalette,
        cmvSummaryPalette, mdpSummaryPalette, cmvPercentChangePalette, speedSummaryPalette, temperatureDetailPalette);
    }

    #endregion


    /* COMMENTED OUT UNTIL OTHER FILTERS REQUIRED - simplified version below as a GET
  /// <summary>
  /// Gets elevation statistics from Raptor.
  /// </summary>
  /// <param name="request">The request for elevation statistics request to Raptor</param>
  /// <returns></returns>
  /// <executor>ElevationStatisticsExecutor</executor>
  [ProjectIdVerifier]
  [ProjectUidVerifier]
  [Route("api/v2/compaction/elevationrange")]
  [HttpPost]
  public ElevationStatisticsResult PostElevationRange([FromBody] CompactionElevationRangeRequest request)
  {
    request.Validate();

    LiftBuildSettings liftSettings;
    try
    {
      liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
    }
    catch (Exception ex)
    {
      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          ex.Message));
    }
    var layerMethod = request.filter == null || !request.filter.layerNumber.HasValue ? (FilterLayerMethod?)null : FilterLayerMethod.TagfileLayerNumber;

    Filter filter = request.filter == null ? null : Filter.CreateFilter(null, null, null, request.filter.startUTC, request.filter.endUTC,
      request.filter.onMachineDesignID, null, request.filter.vibeStateOn, null, request.filter.elevationType,
      null, null, null, null, null, null, null, null, null, layerMethod, null, null,
      request.filter.layerNumber, null, request.filter.contributingMachines, null, null, null, null, null, null, null);
    filter?.Validate();

    ElevationStatisticsRequest statsRequest =
      ElevationStatisticsRequest.CreateElevationStatisticsRequest(request.projectId.Value, null, filter, 0,
        liftSettings);
    statsRequest.Validate();
    return
        RequestExecutorContainer.Build<ElevationStatisticsExecutor>(logger, raptorClient, null).Process(statsRequest)
            as ElevationStatisticsResult;
  }
  */


    /// <summary>
    /// Get elevation range from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>Elevation statistics</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/elevationrange")]
    [HttpGet]
    public ElevationStatisticsResult GetElevationRange([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetElevationRange: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      ElevationStatisticsRequest statsRequest =
        ElevationStatisticsRequest.CreateElevationStatisticsRequest(projectId.Value, null, filter, 0, liftSettings);
      statsRequest.Validate();
  
      try
      {
        var result =
          RequestExecutorContainer.Build<ElevationStatisticsExecutor>(logger, raptorClient, null).Process(statsRequest)
            as ElevationStatisticsResult;
        log.LogInformation("GetElevationRange result: " + JsonConvert.SerializeObject(result));
        return result;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
      finally
      {
        log.LogInformation("GetElevationRange returned: " + Response.StatusCode);
      }

    }

    // TEMP v2 copy of v1 until we have a simplified contract for Compaction
    /// <summary>
    /// Gets project statistics from Raptor.
    /// </summary>
    /// <param name="request">The request for statistics request to Raptor</param>
    /// <returns></returns>
    /// <executor>ProjectStatisticsExecutor</executor>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/projectstatistics")]
    [HttpPost]
    public ProjectStatisticsResult PostProjectStatistics([FromBody] ProjectStatisticsRequest request)
    {
      log.LogInformation("PostProjectStatistics: " + JsonConvert.SerializeObject(request));
      request.Validate();
      try
      {
        var returnResult =
            RequestExecutorContainer.Build<ProjectStatisticsExecutor>(logger, raptorClient, null).Process(request)
                as ProjectStatisticsResult;
        log.LogInformation("PostProjectStatistics result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
      finally
      {
        log.LogInformation("PostProjectStatistics returned: " + Response.StatusCode);
      }
    }


    #region Tiles
    /// <summary>
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as 
    /// elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="request">A representation of the tile rendering request.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/tiles")]
    [HttpPost]
    public TileResult PostTile([FromBody] CompactionTileRequest request)
    {
      log.LogDebug("PostTile: " + JsonConvert.SerializeObject(request));

      var tileResult = GetTile(request);
      return tileResult;
    }


    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="request">A representation of the tile rendering request.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. 
    /// If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered 
    /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to 
    /// indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
    /// (number of cells across a subgrid) * 0.34 (default width in meters of a single cell).
    /// </returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/tiles/png")]
    [HttpPost]
    public FileResult PostTileRaw([FromBody] CompactionTileRequest request)
    {
      log.LogDebug("PostTileRaw: " + JsonConvert.SerializeObject(request));

      var tileResult = GetTile(request);
      if (tileResult != null)
      {
        Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());
        return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
      }
 
      throw new ServiceException(HttpStatusCode.NoContent,
           new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
               "Raptor failed to return a tile"));
    }

    /// <summary>
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as 
    /// elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="SERVICE">WMS parameter - value WMS</param>
    /// <param name="VERSION">WMS parameter - value 1.3</param>
    /// <param name="REQUEST">WMS parameter - value GetMap</param>
    /// <param name="FORMAT">WMS parameter - value image/png</param>
    /// <param name="TRANSPARENT">WMS parameter - value true</param>
    /// <param name="LAYERS">WMS parameter - value Layers</param>
    /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/tiles")]
    [HttpGet]
    public TileResult GetTile(
      [FromQuery] string SERVICE,
      [FromQuery] string VERSION,
      [FromQuery] string REQUEST,
      [FromQuery] string FORMAT,
      [FromQuery] string TRANSPARENT,
      [FromQuery] string LAYERS,
      [FromQuery] int WIDTH,
      [FromQuery] int HEIGHT,
      [FromQuery] string BBOX,
      [FromQuery] long? projectId, 
      [FromQuery] Guid? projectUid, 
      [FromQuery] DisplayMode mode, 
      [FromQuery] DateTime? startUtc, 
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn, 
      [FromQuery] ElevationType? elevationType, 
      [FromQuery] int? layerNumber, 
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogDebug("GetTile: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      MachineDetails machine = null;
      if (assetID.HasValue || !string.IsNullOrEmpty(machineName) || isJohnDoe.HasValue)
      {
        if (!assetID.HasValue || string.IsNullOrEmpty(machineName) || !isJohnDoe.Value)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
           new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
               "If using a machine, asset ID machine name and john doe flag must be provided"));
        }
        machine = MachineDetails.CreateMachineDetails(assetID.Value, machineName, isJohnDoe.Value);
      }
      List<MachineDetails> machines = machine == null ? null : new List<MachineDetails> { machine };

      double blLong = 0, blLat = 0, trLong = 0, trLat = 0;
      GetBoundingBox(BBOX, out blLong, out blLat, out trLong, out trLat);

      CompactionTileRequest request = CompactionTileRequest.CreateTileRequest(projectId.Value, mode, null, 
        CompactionFilter.CreateFilter(startUtc, endUtc, vibeStateOn, elevationType, layerNumber, onMachineDesignId, machines),
        BoundingBox2DLatLon.CreateBoundingBox2DLatLon(blLong, blLat, trLong, trLat), (ushort)WIDTH, (ushort)HEIGHT);
      var tileResult = GetTile(request);
      return tileResult;
    }


    /// <summary>
    /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="SERVICE">WMS parameter - value WMS</param>
    /// <param name="VERSION">WMS parameter - value 1.3</param>
    /// <param name="REQUEST">WMS parameter - value GetMap</param>
    /// <param name="FORMAT">WMS parameter - value image/png</param>
    /// <param name="TRANSPARENT">WMS parameter - value true</param>
    /// <param name="LAYERS">WMS parameter - value Layers</param>
    /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. 
    /// If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered 
    /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to 
    /// indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
    /// (number of cells across a subgrid) * 0.34 (default width in meters of a single cell).
    /// </returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/tiles/png")]
    [HttpGet]
    public FileResult GetTileRaw(
      [FromQuery] string SERVICE,
      [FromQuery] string VERSION,
      [FromQuery] string REQUEST,
      [FromQuery] string FORMAT,
      [FromQuery] string TRANSPARENT,
      [FromQuery] string LAYERS,
      [FromQuery] int WIDTH,
      [FromQuery] int HEIGHT,
      [FromQuery] string BBOX,
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DisplayMode mode,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogDebug("GetTileRaw: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      MachineDetails machine = null;
      if (assetID.HasValue || !string.IsNullOrEmpty(machineName) || isJohnDoe.HasValue)
      {
        if (!assetID.HasValue || string.IsNullOrEmpty(machineName) || !isJohnDoe.Value)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
           new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
               "If using a machine, asset ID machine name and john doe flag must be provided"));
        }
        machine = MachineDetails.CreateMachineDetails(assetID.Value, machineName, isJohnDoe.Value);
      }
      List<MachineDetails> machines = machine == null ? null : new List<MachineDetails> { machine };

      double blLong = 0, blLat = 0, trLong = 0, trLat = 0;
      GetBoundingBox(BBOX, out blLong, out blLat, out trLong, out trLat);
      log.LogDebug("BBOX in radians: blLong=" + blLong + ",blLat=" + blLat + ",trLong=" + trLong + ",trLat=" + trLat);
      CompactionTileRequest request = CompactionTileRequest.CreateTileRequest(projectId.Value, mode, null,
        CompactionFilter.CreateFilter(startUtc, endUtc, vibeStateOn, elevationType, layerNumber, onMachineDesignId, machines),
        BoundingBox2DLatLon.CreateBoundingBox2DLatLon(blLong, blLat, trLong, trLat), (ushort)WIDTH, (ushort)HEIGHT);
      var tileResult = GetTile(request);
      if (tileResult != null)
      {
        Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());
        Response.Headers.Add("Cache-Control", "public");
        Response.Headers.Add("Expires", DateTime.Now.AddMinutes(15).ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
        return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
      }

      throw new ServiceException(HttpStatusCode.NoContent,
           new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
               "Raptor failed to return a tile"));
    }

    /// <summary>
    /// Get the bounding box values from the query parameter
    /// </summary>
    /// <param name="bbox">The query parameter containing the bounding box in decimal degrees</param>
    /// <param name="blLong">Bottom left longitude in radians</param>
    /// <param name="blLat">Bottom left latitude in radians</param>
    /// <param name="trLong">Top right longitude in radians</param>
    /// <param name="trLat">Top right latitude in radians</param>
    private void GetBoundingBox(string bbox, out double blLong, out double blLat, out double trLong, out double trLat)
    {
      blLong = 0;
      blLat = 0;
      trLong = 0;
      trLat = 0;

      int count = 0;
      foreach (string s in bbox.Split(','))
      {
        double num;

        if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
           new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
               "Invalid bounding box"));
        }
        num = num * Math.PI / 180.0;//convert decimal degrees to radians
        //Latitude Must be in range -pi/2 to pi/2 and longitude in the range -pi to pi
        if (count == 0 || count == 2)
        {
          if (num < -Math.PI / 2)
          {
            num = num + Math.PI;
          }
          else if (num > Math.PI / 2)
          {
            num = num - Math.PI;
          }
        }
        if (count == 1 || count == 3)
        {
          if (num < -Math.PI)
          {
            num = num + 2 * Math.PI;
          }
          else if (num > Math.PI)
          {
            num = num - 2 * Math.PI;
          }
        }

        switch (count++)
        {
          case 0: blLat = num; break;
          case 1: blLong = num; break;
          case 2: trLat = num; break;
          case 3: trLong = num; break;
        }
      }
    }

    /// <summary>
    /// Gets the requested tile from Raptor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private TileResult GetTile(CompactionTileRequest request)
    {
      request.Validate();

      LiftBuildSettings liftSettings;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      var layerMethod = request.filter == null || !request.filter.layerNumber.HasValue ? (FilterLayerMethod?)null : FilterLayerMethod.TagfileLayerNumber;

      Filter filter = request.filter == null ? null : Filter.CreateFilter(null, null, null, request.filter.startUTC, request.filter.endUTC,
        request.filter.onMachineDesignID, null, request.filter.vibeStateOn, null, request.filter.elevationType,
        null, null, null, null, null, null, null, null, null, layerMethod, null, null,
        request.filter.layerNumber, null, request.filter.contributingMachines, null, null, null, null, null, null, null);
      filter?.Validate();

      if (!layerMethod.HasValue) layerMethod = FilterLayerMethod.None;

      TileRequest tileRequest = TileRequest.CreateTileRequest(request.projectId.Value, null, request.mode, request.palette,
        liftSettings, RaptorConverters.VolumesType.None, 0, null, filter, 0, null, 0, layerMethod.Value,
        request.boundBoxLL, null, request.width, request.height, 0);
      tileRequest.Validate();
      var tileResult = RequestExecutorContainer.Build<TilesExecutor>(logger, raptorClient, null).Process(tileRequest) as TileResult;
      return tileResult;
    }
    #endregion

  }
}
