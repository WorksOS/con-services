using System;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.ProductionData.Contracts;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;


namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
{
  public class MachinesController : Controller, IMachinesContract
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

    /// <summary>
    /// Constructor with injected raptor client and logger
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    public MachinesController(IASNodeClient raptorClient, ILoggerFactory logger, IAuthenticatedProjectsStore authProjectsStore)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<MachinesController>();
      this.authProjectsStore = authProjectsStore;
    }

    // GET: api/Machines
    /// <summary>
    /// Gets details such as last known position, design, status etc. for machines for a specified project
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>List of machines for the project</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [Route("api/v1/projects/{projectId}/machines")]
    [HttpGet]

    public MachineExecutionResult Get([FromRoute] long projectId)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();
      return RequestExecutorContainer.Build<GetMachineIdsExecutor>(logger, raptorClient, null).Process(Id) as MachineExecutionResult;
    }

    /// <summary>
    /// Gets details such as last known position, design, status etc. for machines for a specified project with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List of machines for the project</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/projects/{projectUid}/machines")]
    [HttpGet]

    public MachineExecutionResult Get([FromRoute] Guid projectUid)
    {
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;

      ProjectID Id = ProjectID.CreateProjectID(ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore), projectUid);
      Id.Validate();
      return RequestExecutorContainer.Build<GetMachineIdsExecutor>(logger, raptorClient, null).Process(Id) as MachineExecutionResult;
    }

    // GET: api/Machines
    /// <summary>
    ///Gets details such as last known position, design, status etc. for machines for a specified machine (must have contributed data to the project)
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="machineId">The machine identifier.</param>
    /// <returns>Info about machine</returns>
    /// <executor>GetMachineIdsExecutor</executor> 
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [Route("api/v1/projects/{projectId}/machines/{machineId}")]
    [HttpGet]

    public ContractExecutionResult Get([FromRoute] long projectId, [FromRoute] long machineId)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();
      MachineExecutionResult result =
          RequestExecutorContainer.Build<GetMachineIdsExecutor>(logger, raptorClient, null).Process(Id) as MachineExecutionResult;
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
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/projects/{projectUid}/machines/{machineId}")]
    [HttpGet]

    public ContractExecutionResult Get([FromRoute] Guid projectUid, [FromRoute] long machineId)
    {
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;

      ProjectID Id = ProjectID.CreateProjectID(ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore), projectUid);
      Id.Validate();
      MachineExecutionResult result =
          RequestExecutorContainer.Build<GetMachineIdsExecutor>(logger, raptorClient, null).Process(Id) as MachineExecutionResult;
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
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [Route("api/v1/projects/{projectId}/machinedesigns")]
    [HttpGet]

    public MachineDesignsExecutionResult GetMachineDesigns([FromRoute] long projectId)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();
      return RequestExecutorContainer.Build<GetMachineDesignsExecutor>(logger, raptorClient, null).Process(Id) as MachineDesignsExecutionResult;
    }

    // GET: api/Machines/Designs
    /// <summary>
    /// Gets On Machine designs for the selected datamodel with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List with all available OnMachine designs in the selected datamodel as reported to Raptor via tag files.</returns>
    /// <executor>GetMachineDesignsExecutor</executor> 
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/projects/{projectUid}/machinedesigns")]
    [HttpGet]

    public MachineDesignsExecutionResult GetMachineDesigns([FromRoute] Guid projectUid)
    {
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;

      ProjectID Id = ProjectID.CreateProjectID(ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore), projectUid);
      Id.Validate();
      return RequestExecutorContainer.Build<GetMachineDesignsExecutor>(logger, raptorClient, null).Process(Id) as MachineDesignsExecutionResult;
    }

    /// <summary>
    /// Gets On Machine liftids for all machines for the selected datamodel
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>List with all available OnMachine layerids in the selected datamodel.</returns>
    /// <executor>GetLayerIdsExecutor</executor> 
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [Route("api/v1/projects/{projectId}/liftids")]
    [HttpGet]

    public LayerIdsExecutionResult GetMachineLayerIds([FromRoute] long projectId)
    {
      ProjectID Id = ProjectID.CreateProjectID(projectId);
      Id.Validate();
      return RequestExecutorContainer.Build<GetLayerIdsExecutor>(logger, raptorClient, null).Process(Id) as LayerIdsExecutionResult;
    }

    /// <summary>
    /// Gets On Machine liftids for all machines for the selected datamodel with a unique identifier
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <returns>List with all available OnMachine layerids in the selected datamodel.</returns>
    /// <executor>GetLayerIdsExecutor</executor> 
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/projects/{projectUid}/liftids")]
    [HttpGet]

    public LayerIdsExecutionResult GetMachineLayerIds([FromRoute] Guid projectUid)
    {
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;

      ProjectID Id = ProjectID.CreateProjectID(ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore), projectUid);
      Id.Validate();
      return RequestExecutorContainer.Build<GetLayerIdsExecutor>(logger, raptorClient, null).Process(Id) as LayerIdsExecutionResult;
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

    public MachineLayerIdsExecutionResult GetMachineLifts([FromRoute] Guid projectUid, [FromQuery] string startUtc = null, [FromQuery] string endUtc = null)
    {
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;

      ProjectID Id = ProjectID.CreateProjectID(ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore), projectUid);
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

      var layerIdsResult = RequestExecutorContainer.Build<GetLayerIdsExecutor>(logger, raptorClient, null).Process(Id) as LayerIdsExecutionResult;
      var machineResult = RequestExecutorContainer.Build<GetMachineIdsExecutor>(logger, raptorClient, null).Process(Id) as MachineExecutionResult;

      var liftDetailsList = new List<MachineLiftDetails>();
      foreach (var machine in machineResult.MachineStatuses)
      {
        List<LayerIdDetails> filteredLayers =
            layerIdsResult.LayerIdDetailsArray.Where(
                layer =>
                    layer.AssetId == machine.assetID &&
                    isDateRangeOverlapping(layer.StartDate, layer.EndDate, beginUtc, finishUtc)).ToList();
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
    private bool isDateRangeOverlapping(DateTime startDate1, DateTime endDate1, DateTime? startDate2, DateTime? endDate2)
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
