using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class MachinesController : Controller, IMachinesContract
  {
#if RAPTOR
    private readonly IASNodeClient _raptorClient;
#endif

    private readonly ILoggerFactory _logger;
    private readonly ILogger _log;
    private readonly IConfigurationStore _configStore;
    private readonly ITRexCompactionDataProxy _trexCompactionDataProxy;
    private IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders(true);
    private string CustomerUid => ((RaptorPrincipal) Request.HttpContext.User).CustomerUid;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MachinesController(
#if RAPTOR
      IASNodeClient raptorClient,
#endif
      ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy
      )
    {
#if RAPTOR
      _raptorClient = raptorClient;
#endif
      _logger = logger;
      _log = logger.CreateLogger<MachinesController>();
      _configStore = configStore;
      _trexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Gets details such as last known position, design, status etc. for machines for a specified project
    /// </summary>
    /// <returns>List of machines for the project</returns>
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/machines")]
    [HttpGet]
    public async Task<MachineExecutionResult> GetMachinesOnProject([FromRoute] long projectId)
    {
      _log.LogInformation($"{nameof(GetMachinesOnProject)} Request. projectId: {projectId}");

      var projectIds = new ProjectIDs(projectId, await ((RaptorPrincipal)User).GetProjectUid(projectId));
      projectIds.Validate();

      var response = await RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy, 
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as MachineExecutionResult;

      _log.LogInformation($"{nameof(GetMachinesOnProject)} Response: {JsonConvert.SerializeObject(response)}");
      return response;
    }

    /// <summary>
    /// Gets details such as last known position, design, status etc. for machines for a specified project with a unique identifier
    /// </summary>
    /// <returns>List of machines for the project</returns>
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machines")]
    [HttpGet]
    public async Task<MachineExecutionResult> GetMachinesOnProject([FromRoute] Guid projectUid)
    {
      _log.LogInformation($"{nameof(GetMachinesOnProject)} Request. projectUid: {projectUid}");

      var projectIds = new ProjectIDs(await((RaptorPrincipal)User).GetLegacyProjectId(projectUid), projectUid);
      projectIds.Validate();

      var response = await RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy, 
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as MachineExecutionResult;

      _log.LogInformation($"{nameof(GetMachinesOnProject)} Response: {JsonConvert.SerializeObject(response)}");
      return response;
    }

    // GET: api/Machines
    /// <summary>
    ///Gets details such as last known position, design, status etc. for machines for a specified machine (must have contributed data to the project)
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="machineId">The machine identifier.</param>
    /// <returns>Info about machine</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/machines/{machineId}")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetMachineOnProject([FromRoute] long projectId, [FromRoute] long machineId)
    {
      _log.LogInformation($"{nameof(GetMachineOnProject)} Request. projectId: {projectId} machineId: {machineId}");

      var projectIds = new ProjectIDs(projectId, await ((RaptorPrincipal)User).GetProjectUid(projectId));
      projectIds.Validate();

      var response = await RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy, 
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as MachineExecutionResult;

      _log.LogInformation($"{nameof(GetMachineOnProject)} Response: machineIdFilter {machineId} responsePreFilter {JsonConvert.SerializeObject(response)}");
      response?.FilterByMachineId(machineId);

      return response;
    }

    /// <summary>
    ///Gets details such as last known position, design, status etc. for machines for a specified machine (must have contributed data to the project with a unique identifier)
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="machineId">The machine identifier.</param>
    /// <returns>Info about machine</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machines/{machineId}")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetMachineOnProject([FromRoute] Guid projectUid, [FromRoute] long machineId)
    {
      _log.LogInformation($"{nameof(GetMachineOnProject)} Request. projectUid: {projectUid} machineId: {machineId}");

      var projectIds = new ProjectIDs(await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid), projectUid);
      projectIds.Validate();

      var response = await RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as MachineExecutionResult;

      _log.LogInformation($"{nameof(GetMachineOnProject)} Response: machineIdFilter {machineId} responsePreFilter {JsonConvert.SerializeObject(response)}");
      response?.FilterByMachineId(machineId);

      return response;
    }

    /// <summary>
    ///Gets details such as last known position, design, status etc. for machines for a specified machine (must have contributed data to the project with a unique identifier)
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="machineUid">The machine identifier.</param>
    /// <returns>Info about machine</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v3/projects/{projectUid}/machines/{machineUid}")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetMachineOnProject([FromRoute] Guid projectUid, [FromRoute] Guid machineUid)
    {
      _log.LogInformation($"{nameof(GetMachineOnProject)} Request. projectUid: {projectUid} machineUid: {machineUid}");

      var projectIds = new ProjectIDs(await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid), projectUid);
      projectIds.Validate();

      var response = await RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as MachineExecutionResult;

      _log.LogInformation($"{nameof(GetMachineOnProject)} Response: machineUidFilter {machineUid} responsePreFilter {JsonConvert.SerializeObject(response)}");
      response?.FilterByMachineUid(machineUid);

      return response;
    }

    // GET: api/Machines/AssetOnDesignPeriods
    /// <summary>
    /// Gets On Machine assetOnDesigns for the selected datamodel
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>List with all available OnMachine assetOnDesigns in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetAssetOnDesignPeriodsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/machinedesigns")]
    [HttpGet]
    public async Task<MachineDesignsResult> GetMachineDesigns([FromRoute] long projectId)
    {
      _log.LogInformation($"{nameof(GetMachineDesigns)} Request. projectId: {projectId}");

      var projectIds = new ProjectIDs(projectId, await ((RaptorPrincipal)User).GetProjectUid(projectId));
      projectIds.Validate();

      var result = await RequestExecutorContainerFactory.Build<GetAssetOnDesignPeriodsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      var response = CreateUniqueMachineDesignList(result);

      _log.LogInformation($"{nameof(GetMachineDesigns)} Response: {JsonConvert.SerializeObject(response)}");
      return response;
    }

    // GET: api/Machines/AssetOnDesignPeriods
    /// <summary>
    /// Gets On Machine assetOnDesigns for the selected datamodel with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List with all available OnMachine assetOnDesigns in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetAssetOnDesignPeriodsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machinedesigns")]
    [HttpGet]
    public async Task<MachineDesignsResult> GetMachineDesigns([FromRoute] Guid projectUid)
    {
      _log.LogInformation($"{nameof(GetMachineDesigns)} Request. projectUid: {projectUid}");

      var projectIds = new ProjectIDs(await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid), projectUid);
      projectIds.Validate();

      var result = await RequestExecutorContainerFactory.Build<GetAssetOnDesignPeriodsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as MachineDesignsExecutionResult;

      _log.LogDebug($"{nameof(GetMachineDesigns)} MachineDesignsExecutionResult: {result}");
      var response = CreateUniqueMachineDesignList(result);
      _log.LogDebug($"{nameof(GetMachineDesigns)} Response: {response}");
      return response;
    }

    /// <summary>
    /// Creates a unique list of all assetOnDesigns for all machines
    /// </summary>
    private MachineDesignsResult CreateUniqueMachineDesignList(MachineDesignsExecutionResult result)
    {
      return new MachineDesignsResult(RemoveDuplicateMachineDesigns(result.AssetOnDesignPeriods));
    }

    /// <summary>
    /// Filters out duplicate assetOnDesigns where the tRex/raptor internal OnMachineDesignId and name match
    ///     NOTE that there is no way to match a machine design (which comes from a tag file)
    ///         with an imported design,
    ///         Therefore this id is unique to tRex OR raptor.
    ///         I don't believe there will/can be any attempt to sync these, so don't mix/match tRex/Raptor calls.
    /// </summary>
    private List<AssetOnDesignPeriod> RemoveDuplicateMachineDesigns(List<AssetOnDesignPeriod> assetsOnDesignPeriods)
    {
      // TRex won't have machineDesignId, but DesignName should be unique
      return assetsOnDesignPeriods.Distinct().OrderBy(d => d.OnMachineDesignId).ThenBy(n => n.OnMachineDesignName).ToList();
    }

    /// <summary>
    /// Gets On Machine assetOnDesigns by machine and date range for the selected project
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="startUtc">The start date/time in UTC.</param>
    /// <param name="endUtc">The end date/time in UTC.</param>
    /// <returns>List with all available OnMachine assetOnDesigns in the selected project as reported to Raptor via tag files.</returns>
    /// <executor>GetAssetOnDesignPeriodsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machinedesigndetails")]
    [HttpGet]
    public async Task<MachineDesignDetailsExecutionResult> GetMachineDesignByDateRangeDetails([FromRoute] Guid projectUid,
      [FromQuery] string startUtc, [FromQuery] string endUtc)
    {
      _log.LogInformation($"{nameof(GetMachineDesignByDateRangeDetails)} Request. projectUid: {projectUid} startUtc: {startUtc} endUtc: {endUtc}");

      var projectIds = new ProjectIDs(await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid), projectUid);
      projectIds.Validate();

      ValidateDates(startUtc, endUtc, out var beginUtc, out var finishUtc);

      var designsResultTask = RequestExecutorContainerFactory.Build<GetAssetOnDesignPeriodsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds);

      var machineResultTask = RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds);

      await Task.WhenAll(designsResultTask, machineResultTask);

      var designsResult = await designsResultTask as MachineDesignsExecutionResult;
      var machineResult = await machineResultTask as MachineExecutionResult;

      _log.LogDebug($"{nameof(GetMachineDesignByDateRangeDetails)} MachineDesignsExecutionResult: {JsonConvert.SerializeObject(designsResult)}");
      _log.LogDebug($"{nameof(GetMachineDesignByDateRangeDetails)} MachineExecutionResult: {JsonConvert.SerializeObject(machineResult)}");

      // for this pairing to work, we need both executors to be using the same source: Raptor/TRex,
      //  otherwise there will be a mismatch of OnMachineDesignId v.s. Uid.
      var designDetailsList = new List<MachineDesignDetails>();
      if (machineResult != null && designsResult != null)
        foreach (var machine in machineResult.MachineStatuses)
        {
          var filteredDesigns =
            designsResult.AssetOnDesignPeriods.Where(
              design =>
                (
                  design.AssetUid.HasValue
                    ? design.AssetUid == machine.AssetUid
                    : design.MachineId == machine.AssetId
                 
                ) &&
                IsDateRangeOverlapping(design.StartDate, design.EndDate, beginUtc, finishUtc)).ToList();

          if (filteredDesigns.Count > 0)
          {
            designDetailsList.Add(new MachineDesignDetails(
              machine.AssetId, machine.MachineName, machine.IsJohnDoe,
              RemoveDuplicateMachineDesigns(filteredDesigns).ToArray(), machine.AssetUid));
          }
        }

      var response = new MachineDesignDetailsExecutionResult(designDetailsList);
      _log.LogInformation($"{nameof(GetMachineDesignByDateRangeDetails)} Response. {JsonConvert.SerializeObject(response)}");
      return response;
    }

    /// <summary>
    /// Gets On Machine lift ids for all machines for the selected datamodel
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>List with all available OnMachine layer ids in the selected datamodel.</returns>
    /// <executor>GetAssetOnDesignLayerPeriodsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/liftids")]
    [HttpGet]
    public async Task<AssetOnDesignLayerPeriodsExecutionResult> GetMachineOnDesignLayerPeriods([FromRoute] long projectId)
    {
      _log.LogInformation($"{nameof(GetMachineOnDesignLayerPeriods)} Request. projectId: {projectId}");

      var projectIds = new ProjectIDs(projectId, await ((RaptorPrincipal)User).GetProjectUid(projectId));
      projectIds.Validate();

      var response = await RequestExecutorContainerFactory.Build<GetAssetOnDesignLayerPeriodsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;

      _log.LogInformation($"{nameof(GetMachineOnDesignLayerPeriods)} Response. {JsonConvert.SerializeObject(response)}");
      return response;
    }

    /// <summary>
    /// Gets On Machine lift ids for all machines for the selected datamodel with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List with all available OnMachine layer ids in the selected datamodel.</returns>
    /// <executor>GetAssetOnDesignLayerPeriodsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/liftids")]
    [HttpGet]
    public async Task<AssetOnDesignLayerPeriodsExecutionResult> GetMachineOnDesignLayerPeriods([FromRoute] Guid projectUid)
    {
      _log.LogInformation($"{nameof(GetMachineOnDesignLayerPeriods)} Request. projectUid: {projectUid}");

      var projectIds = new ProjectIDs(await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid), projectUid);
      projectIds.Validate();

      var response = await RequestExecutorContainerFactory.Build<GetAssetOnDesignLayerPeriodsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;

      _log.LogDebug($"{nameof(GetMachineOnDesignLayerPeriods)} Response: {JsonConvert.SerializeObject(response)}");
      return response;
    }

    /// <summary>
    /// Gets On Machine lift ids for each machine for the selected datamodel for the specified date range.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="startUtc">The start date/time in UTC.</param>
    /// <param name="endUtc">The end date/time in UTC.</param>
    /// <returns>List with all available lift ids for each machine in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetAssetOnDesignLayerPeriodsExecutor</executor> 
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/machinelifts")]
    [HttpGet]
    public async Task<MachineLayerIdsExecutionResult> GetMachineLiftsByDateRange([FromRoute] long projectId,
      [FromQuery] string startUtc = null, [FromQuery] string endUtc = null)
    {
      _log.LogInformation($"{nameof(GetMachineLiftsByDateRange)} Request. projectId: {projectId} startUtc: {startUtc} endUtc: {endUtc}");

      var projectIds = new ProjectIDs(projectId, await ((RaptorPrincipal)User).GetProjectUid(projectId));
      projectIds.Validate();

      var response = await GetMachineLiftsWith(projectIds, startUtc, endUtc);

      _log.LogDebug($"{nameof(GetMachineLiftsByDateRange)} Response: {JsonConvert.SerializeObject(response)}");
      return response;
    }

    /// <summary>
    /// Gets On Machine lift ids for each machine for the selected datamodel with a unique identifier for the specified date range.
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="startUtc">The start date/time in UTC.</param>
    /// <param name="endUtc">The end date/time in UTC.</param>
    /// <returns>List with all available lift ids for each machine in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetAssetOnDesignLayerPeriodsExecutor</executor> 
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machinelifts")]
    [HttpGet]
    public async Task<MachineLayerIdsExecutionResult> GetMachineLiftsByDateRange([FromRoute] Guid projectUid,
      [FromQuery] string startUtc = null, [FromQuery] string endUtc = null)
    {
      _log.LogInformation($"{nameof(GetMachineLiftsByDateRange)} Request. projectUid: {projectUid} startUtc: {startUtc} endUtc: {endUtc}");

      var projectIds = new ProjectIDs(await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid), projectUid);
      projectIds.Validate();

      var response =  await GetMachineLiftsWith(projectIds, startUtc, endUtc);

      _log.LogDebug($"{nameof(GetMachineLiftsByDateRange)} Response: {JsonConvert.SerializeObject(response)}");
      return response;
    }

    private async Task<MachineLayerIdsExecutionResult> GetMachineLiftsWith(ProjectIDs projectIds, string startUtc, string endUtc)
    {
      //Note: we use strings in the uri because the framework converts to local time although we are using UTC format.
      //Posts on the internet suggests using JsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc
      //and IsoDateTimeConverter but that didn't fix the problem.
      ValidateDates(startUtc, endUtc, out var beginUtc, out var finishUtc);

      var layerIdsResultTask = RequestExecutorContainerFactory.Build<GetAssetOnDesignLayerPeriodsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds);

      var machineResultTask = RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(_logger,
#if RAPTOR
          _raptorClient,
#endif
          configStore: _configStore, trexCompactionDataProxy: _trexCompactionDataProxy,
          customHeaders: CustomHeaders, customerUid: CustomerUid)
        .ProcessAsync(projectIds);

      await Task.WhenAll(layerIdsResultTask, machineResultTask);

      var layerIdsResult = await layerIdsResultTask as AssetOnDesignLayerPeriodsExecutionResult;
      var machineResult = await machineResultTask as MachineExecutionResult;

      _log.LogDebug($"{nameof(GetMachineLiftsWith)} AssetOnDesignLayerPeriodsExecutionResult: {layerIdsResult} MachineExecutionResult: {machineResult}");

      if (machineResult?.MachineStatuses == null)
        return new MachineLayerIdsExecutionResult(new List<MachineLiftDetails>());

      var liftDetailsList = new List<MachineLiftDetails>();
      foreach (var machine in machineResult.MachineStatuses)
      {
        var filteredLayers =
          layerIdsResult?.AssetOnDesignLayerPeriods.Where(
            layer => (layer.AssetUid.HasValue ? layer.AssetUid == machine.AssetUid : layer.AssetId == machine.AssetId) &&
                     IsDateRangeOverlapping(layer.StartDate, layer.EndDate, beginUtc, finishUtc)).ToList();

        if (filteredLayers?.Count > 0)
        {
          liftDetailsList.Add(new MachineLiftDetails(
            machine.AssetId, machine.MachineName, machine.IsJohnDoe,
            filteredLayers.Select(f => new LiftDetails
            {
              DesignId = f.OnMachineDesignId,
              DesignName = f.OnMachineDesignName,
              LayerId = f.LayerId,
              EndUtc = f.EndDate
            }).ToArray(), machine.AssetUid));
        }
      }

      return new MachineLayerIdsExecutionResult(liftDetailsList);
    }

    private void ValidateDates(string startUtc, string endUtc, out DateTime? beginUtc, out DateTime? finishUtc)
    {
      beginUtc = ParseUtcDate(startUtc);
      finishUtc = ParseUtcDate(endUtc);

      if (beginUtc.HasValue || finishUtc.HasValue)
      {
        if (beginUtc.HasValue && finishUtc.HasValue)
        {
          if (beginUtc.Value > finishUtc.Value)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "StartUtc must be earlier than endUtc"));
          }
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "If using a date range both dates must be provided"));
        }
      }
    }

    private DateTime? ParseUtcDate(string utcDate)
    {
      if (string.IsNullOrEmpty(utcDate))
        return null;

      DateTime dateResult;

      if (DateTime.TryParseExact(utcDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateResult))
      {
        _log.LogWarning($"{nameof(ParseUtcDate)}: Inappropriate ISO date format being parsed: '{utcDate}' against 'yyyy-MM-ddTHH:mm:ssZ'");
        return dateResult;
      }

      if (DateTime.TryParseExact(utcDate, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateResult))
      {
        return dateResult;
      }

      return null;
    }

    /// <summary>
    /// Determines if two date ranges overlap
    /// </summary>
    /// <param name="startDate1">Start of first date range</param>
    /// <param name="endDate1">End of first date range</param>
    /// <param name="startDate2">Start of second date range</param>
    /// <param name="endDate2">End of second date range</param>
    /// <returns>True if they overlap otherwise false</returns>
    private bool IsDateRangeOverlapping(DateTime startDate1, DateTime endDate1, DateTime? startDate2,
      DateTime? endDate2)
    {
      if (startDate2.HasValue && endDate2.HasValue)
      {
        var noOverlap = endDate2 < startDate1 || startDate2 > endDate1;
        return !noOverlap;
      }

      return true;
    }
  }
}
