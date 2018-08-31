using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
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
    private readonly IASNodeClient raptorClient;
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public MachinesController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    /// <summary>
    /// Gets details such as last known position, design, status etc. for machines for a specified project
    /// </summary>
    /// <returns>List of machines for the project</returns>
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/machines")]
    [HttpGet]

    public MachineExecutionResult Get([FromRoute] long projectId)
    {
      var id = new ProjectID(projectId);
      id.Validate();

      return RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(id) as MachineExecutionResult;
    }

    /// <summary>
    /// Gets details such as last known position, design, status etc. for machines for a specified project with a unique identifier
    /// </summary>
    /// <returns>List of machines for the project</returns>
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machines")]
    [HttpGet]

    public async Task<MachineExecutionResult> Get([FromRoute] Guid projectUid)
    {
      var projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      var id = new ProjectID(projectId, projectUid);
      id.Validate();
      return RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(id) as MachineExecutionResult;
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

    public ContractExecutionResult Get([FromRoute] long projectId, [FromRoute] long machineId)
    {
      var id = new ProjectID(projectId);
      id.Validate();
      var result =
          RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(id) as MachineExecutionResult;
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
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machines/{machineId}")]
    [HttpGet]

    public async Task<ContractExecutionResult> Get([FromRoute] Guid projectUid, [FromRoute] long machineId)
    {
      var projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      var id = new ProjectID(projectId, projectUid);
      id.Validate();
      var result =
          RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(id) as MachineExecutionResult;
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
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/machinedesigns")]
    [HttpGet]

    public MachineDesignsExecutionResult GetMachineDesigns([FromRoute] long projectId)
    {
      var id = new ProjectID(projectId);
      id.Validate();
      var result = RequestExecutorContainerFactory.Build<GetMachineDesignsExecutor>(logger, raptorClient).Process(id) as MachineDesignsExecutionResult;
      return CreateUniqueDesignList(result);
    }

    // GET: api/Machines/Designs
    /// <summary>
    /// Gets On Machine designs for the selected datamodel with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List with all available OnMachine designs in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetMachineDesignsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machinedesigns")]
    [HttpGet]
    public async Task<MachineDesignsExecutionResult> GetMachineDesigns([FromRoute] Guid projectUid)
    {
      var projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      var id = new ProjectID(projectId, projectUid);
      id.Validate();
      var result = RequestExecutorContainerFactory.Build<GetMachineDesignsExecutor>(logger, raptorClient).Process(id) as MachineDesignsExecutionResult;
      return CreateUniqueDesignList(result);
    }

    /// <summary>
    /// Creates a unique list of all designs for all machines
    /// </summary>
    private MachineDesignsExecutionResult CreateUniqueDesignList(MachineDesignsExecutionResult result)
    {
      return MachineDesignsExecutionResult.Create(RemoveDuplicateDesigns(result.Designs));
    }

    /// <summary>
    /// Filters out duplicate designs where the id and name match
    /// </summary>
    private List<DesignName> RemoveDuplicateDesigns(List<DesignName> designNames)
    {
      return designNames.Distinct().OrderBy(d => d.Id).ToList();
    }

    /// <summary>
    /// Gets On Machine designs by machine and date range for the selected project
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="startUtc">The start date/time in UTC.</param>
    /// <param name="endUtc">The end date/time in UTC.</param>
    /// <returns>List with all available OnMachine designs in the selected project as reported to Raptor via tag files.</returns>
    /// <executor>GetMachineDesignsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machinedesigndetails")]
    [HttpGet]
    public async Task<MachineDesignDetailsExecutionResult> GetMachineDesignDetails([FromRoute] Guid projectUid, [FromQuery]string startUtc, [FromQuery]string endUtc)
    {
      var projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      var id = new ProjectID(projectId, projectUid);
      id.Validate();

      DateTime? beginUtc;
      DateTime? finishUtc;
      ValidateDates(startUtc, endUtc, out beginUtc, out finishUtc);

      var designsResult = RequestExecutorContainerFactory.Build<GetMachineDesignsExecutor>(logger, raptorClient).Process(id) as MachineDesignsExecutionResult;
      var machineResult = RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(id) as MachineExecutionResult;

      var designDetailsList = new List<MachineDesignDetails>();
      foreach (var machine in machineResult.MachineStatuses)
      {
        var filteredDesigns =
          designsResult.Designs.Where(
            design =>
              design.MachineId == machine.AssetId &&
              IsDateRangeOverlapping(design.StartDate, design.EndDate, beginUtc, finishUtc)).ToList();
        if (filteredDesigns.Count > 0)
        {
          designDetailsList.Add(MachineDesignDetails.CreateMachineDesignDetails(
            machine.AssetId, machine.MachineName, machine.IsJohnDoe,
            RemoveDuplicateDesigns(filteredDesigns).ToArray()));
        }
      }
      return MachineDesignDetailsExecutionResult.Create(designDetailsList.ToArray());
    }

    /// <summary>
    /// Gets On Machine liftids for all machines for the selected datamodel
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>List with all available OnMachine layerids in the selected datamodel.</returns>
    /// <executor>GetLayerIdsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/liftids")]
    [HttpGet]
    public LayerIdsExecutionResult GetMachineLayerIds([FromRoute] long projectId)
    {
      var id = new ProjectID(projectId);
      id.Validate();
      return RequestExecutorContainerFactory.Build<GetLayerIdsExecutor>(logger, raptorClient).Process(id) as LayerIdsExecutionResult;
    }

    /// <summary>
    /// Gets On Machine liftids for all machines for the selected datamodel with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List with all available OnMachine layerids in the selected datamodel.</returns>
    /// <executor>GetLayerIdsExecutor</executor> 
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/liftids")]
    [HttpGet]
    public async Task<LayerIdsExecutionResult> GetMachineLayerIds([FromRoute] Guid projectUid)
    {
      var projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      var id = new ProjectID(projectId, projectUid);
      id.Validate();
      return RequestExecutorContainerFactory.Build<GetLayerIdsExecutor>(logger, raptorClient).Process(id) as LayerIdsExecutionResult;
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
    [ProjectVerifier]
    [Route("api/v1/projects/{projectId}/machinelifts")]
    [HttpGet]
    public MachineLayerIdsExecutionResult GetMachineLifts([FromRoute] long projectId, [FromQuery] string startUtc = null, [FromQuery] string endUtc = null)
    {
      var id = new ProjectID(projectId);
      id.Validate();

      return GetMachineLiftsWith(id, startUtc, endUtc);
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
    [ProjectVerifier]
    [Route("api/v2/projects/{projectUid}/machinelifts")]
    [HttpGet]
    public async Task<MachineLayerIdsExecutionResult> GetMachineLifts([FromRoute] Guid projectUid, [FromQuery] string startUtc = null, [FromQuery] string endUtc = null)
    {
      var projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      var id = new ProjectID(projectId, projectUid);
      id.Validate();

      return GetMachineLiftsWith(id, startUtc, endUtc);
    }

    private MachineLayerIdsExecutionResult GetMachineLiftsWith(ProjectID id, string startUtc, string endUtc)
    {
      //Note: we use strings in the uri because the framework converts to local time although we are using UTC format.
      //Posts on the internet suggets using JsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc
      //and IsoDateTimeConverter but that didn't fix the problem.
      DateTime? beginUtc;
      DateTime? finishUtc;
      ValidateDates(startUtc, endUtc, out beginUtc, out finishUtc);

      var layerIdsResult = RequestExecutorContainerFactory.Build<GetLayerIdsExecutor>(logger, raptorClient).Process(id) as LayerIdsExecutionResult;
      var machineResult = RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(logger, raptorClient).Process(id) as MachineExecutionResult;

      var liftDetailsList = new List<MachineLiftDetails>();
      foreach (var machine in machineResult.MachineStatuses)
      {
        var filteredLayers =
            layerIdsResult.LayerIdDetailsArray.Where(
                layer =>
                    layer.AssetId == machine.AssetId &&
                    IsDateRangeOverlapping(layer.StartDate, layer.EndDate, beginUtc, finishUtc)).ToList();
        if (filteredLayers.Count > 0)
        {
          liftDetailsList.Add(MachineLiftDetails.CreateMachineLiftDetails(
              machine.AssetId, machine.MachineName, machine.IsJohnDoe,
              filteredLayers.Select(f => new LiftDetails { LayerId = f.LayerId, EndUtc = f.EndDate }).ToArray()));
        }
      }

      return MachineLayerIdsExecutionResult.CreateMachineLayerIdsExecutionResult(liftDetailsList.ToArray());
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
        var noOverlap = endDate2 < startDate1 || startDate2 > endDate1;
        return !noOverlap;
      }
      return true;
    }
  }
}
