using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;


namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class MachinesController : Controller, IMachinesContract
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">LoggerFactory</param>
    public MachinesController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    // GET: api/Machines
    /// <summary>
    /// Gets details such as last known position, design, status etc. for machines for a specified project
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>List of machines for the project</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/projects/{projectId}/machines")]
    [HttpGet]

    public MachineExecutionResult Get([FromRoute] long projectId)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();
      return RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(Id) as MachineExecutionResult;
    }

    /// <summary>
    /// Gets details such as last known position, design, status etc. for machines for a specified project with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List of machines for the project</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v2/projects/{projectUid}/machines")]
    [HttpGet]

    public async Task<MachineExecutionResult> Get([FromRoute] Guid projectUid)
    {
      long projectId = await (User as RaptorPrincipal).GetLegacyProjectId(projectUid);
      ProjectID Id = ProjectID.CreateProjectID(projectId, projectUid);
      Id.Validate();
      return RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(Id) as MachineExecutionResult;
    }

    // GET: api/Machines
    /// <summary>
    ///Gets details such as last known position, design, status etc. for machines for a specified machine (must have contributed data to the project)
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="machineId">The machine identifier.</param>
    /// <returns>Info about machine</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/projects/{projectId}/machines/{machineId}")]
    [HttpGet]

    public ContractExecutionResult Get([FromRoute] long projectId, [FromRoute] long machineId)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();
      MachineExecutionResult result =
          RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(Id) as MachineExecutionResult;
      result.FilterByMachineId(machineId);
      return result;
    }

    /// <summary>
    ///Gets details such as last known position, design, status etc. for machines for a specified machine (must have contributed data to the project with a unique identifier)
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="machineId">The machine identifier.</param>
    /// <returns>Info about machine</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v2/projects/{projectUid}/machines/{machineId}")]
    [HttpGet]

    public async Task<ContractExecutionResult> Get([FromRoute] Guid projectUid, [FromRoute] long machineId)
    {
      long projectId = await (User as RaptorPrincipal).GetLegacyProjectId(projectUid);
      ProjectID Id = ProjectID.CreateProjectID(projectId, projectUid);
      Id.Validate();
      MachineExecutionResult result =
          RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(Id) as MachineExecutionResult;
      result.FilterByMachineId(machineId);
      return result;
    }

    // GET: api/Machines/Designs
    /// <summary>
    /// Gets On Machine designs for the selected datamodel
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>List with all available OnMachine designs in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetMachineDesignsExecutor</executor> 
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/projects/{projectId}/machinedesigns")]
    [HttpGet]

    public MachineDesignsExecutionResult GetMachineDesigns([FromRoute] long projectId)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();
      return RequestExecutorContainerFactory.Build<GetMachineDesignsExecutor>(logger, raptorClient).Process(Id) as MachineDesignsExecutionResult;
    }

    // GET: api/Machines/Designs
    /// <summary>
    /// Gets On Machine designs for the selected datamodel with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List with all available OnMachine designs in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetMachineDesignsExecutor</executor> 
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v2/projects/{projectUid}/machinedesigns")]
    [HttpGet]

    public async Task<MachineDesignsExecutionResult> GetMachineDesigns([FromRoute] Guid projectUid)
    {
      long projectId = await (User as RaptorPrincipal).GetLegacyProjectId(projectUid);
      ProjectID Id = ProjectID.CreateProjectID(projectId, projectUid);
      Id.Validate();
      return RequestExecutorContainerFactory.Build<GetMachineDesignsExecutor>(logger, raptorClient).Process(Id) as MachineDesignsExecutionResult;
    }

    /// <summary>
    /// Gets On Machine liftids for all machines for the selected datamodel
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>List with all available OnMachine layerids in the selected datamodel.</returns>
    /// <executor>GetLayerIdsExecutor</executor> 
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/projects/{projectId}/liftids")]
    [HttpGet]
    public LayerIdsExecutionResult GetMachineLayerIds([FromRoute] long projectId)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();
      return RequestExecutorContainerFactory.Build<GetLayerIdsExecutor>(logger, raptorClient).Process(Id) as LayerIdsExecutionResult;
    }

    /// <summary>
    /// Gets On Machine liftids for all machines for the selected datamodel with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List with all available OnMachine layerids in the selected datamodel.</returns>
    /// <executor>GetLayerIdsExecutor</executor> 
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v2/projects/{projectUid}/liftids")]
    [HttpGet]
    public async Task<LayerIdsExecutionResult> GetMachineLayerIds([FromRoute] Guid projectUid)
    {
      long projectId = await (User as RaptorPrincipal).GetLegacyProjectId(projectUid);
      ProjectID Id = ProjectID.CreateProjectID(projectId, projectUid);
      Id.Validate();
      return RequestExecutorContainerFactory.Build<GetLayerIdsExecutor>(logger, raptorClient).Process(Id) as LayerIdsExecutionResult;
    }

    /// <summary>
    /// Gets On Machine liftids for each machine for the selected datamodel for the specified date range.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="startUtc">The start date/time in UTC.</param>
    /// <param name="endUtc">The end date/time in UTC.</param>
    /// <returns>List with all available liftids for each machine in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetLayerIdsExecutor</executor> 
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectIdVerifier]
    [Route("api/v1/projects/{projectId}/machinelifts")]
    [HttpGet]
    public MachineLayerIdsExecutionResult GetMachineLifts([FromRoute] long projectId, [FromQuery] string startUtc = null, [FromQuery] string endUtc = null)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();

      return GetMachineLiftsWith(Id, startUtc, endUtc);
    }

    /// <summary>
    /// Gets On Machine liftids for each machine for the selected datamodel with a unique identifier for the specified date range.
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="startUtc">The start date/time in UTC.</param>
    /// <param name="endUtc">The end date/time in UTC.</param>
    /// <returns>List with all available liftids for each machine in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetLayerIdsExecutor</executor> 
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/projects/{projectUid}/machinelifts")]
    [HttpGet]
    public async Task<MachineLayerIdsExecutionResult> GetMachineLifts([FromRoute] Guid projectUid, [FromQuery] string startUtc = null, [FromQuery] string endUtc = null)
    {
      long projectId = await (User as RaptorPrincipal).GetLegacyProjectId(projectUid);
      ProjectID Id = ProjectID.CreateProjectID(projectId, projectUid);
      Id.Validate();

      return GetMachineLiftsWith(Id, startUtc, endUtc);
    }

    private MachineLayerIdsExecutionResult GetMachineLiftsWith(ProjectID Id, string startUtc, string endUtc)
    {
      //Note: we use strings in the uri because the framework converts to local time although we are using UTC format.
      //Posts on the internet suggets using JsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc
      //and IsoDateTimeConverter but that didn't fix the problem.
      DateTime? beginUtc = ParseUtcDate(startUtc);
      DateTime? finishUtc = ParseUtcDate(endUtc);

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

      var layerIdsResult = RequestExecutorContainerFactory.Build<GetLayerIdsExecutor>(logger, raptorClient).Process(Id) as LayerIdsExecutionResult;
      var machineResult = RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(Id) as MachineExecutionResult;

      var liftDetailsList = new List<MachineLiftDetails>();
      foreach (var machine in machineResult.MachineStatuses)
      {
        List<LayerIdDetails> filteredLayers =
            layerIdsResult.LayerIdDetailsArray.Where(
                layer =>
                    layer.AssetId == machine.assetID &&
                    IsDateRangeOverlapping(layer.StartDate, layer.EndDate, beginUtc, finishUtc)).ToList();
        if (filteredLayers.Count > 0)
        {
          liftDetailsList.Add(MachineLiftDetails.CreateMachineLiftDetails(
              machine.assetID, machine.machineName, machine.isJohnDoe,
              filteredLayers.Select(f => new LiftDetails { layerId = f.LayerId, endUtc = f.EndDate }).ToArray()));
        }
      }

      return MachineLayerIdsExecutionResult.CreateMachineLayerIdsExecutionResult(liftDetailsList.ToArray());
    }

    private DateTime? ParseUtcDate(string utcDate)
    {
      return string.IsNullOrEmpty(utcDate)
            ? (DateTime?)null
            : DateTime.ParseExact(utcDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
    }

    /// <summary>
    /// Determines if two date ranges overlap
    /// </summary>
    /// <param name="startDate1">Start of first date range</param>
    /// <param name="endDate1">End of first date range</param>
    /// <param name="startDate2">Start of second date range</param>
    /// <param name="endDate2">End of second date range</param>
    /// <returns>True if they overlap otherwise false</returns>
    private bool IsDateRangeOverlapping(DateTime startDate1, DateTime endDate1, DateTime? startDate2, DateTime? endDate2)
    {
      if (startDate2.HasValue && endDate2.HasValue)
      {
        bool noOverlap = endDate2 < startDate1 || startDate2 > endDate1;
        return !noOverlap;
      }
      return true;
    }
  }
}