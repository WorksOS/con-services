using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.DI;
using VSS.TRex.Events.Models;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller to get production data edits.
  /// Add/Remove data edit endpoints use the mutable endpoint (at present VSS.TRex.Mutable.Gateway.WebApi)
  /// </summary>
  public class EditDataController : BaseController
  {
    /// <summary>
    /// Constructor with injection
    /// </summary>
    public EditDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<EditDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Gets the list of applied production data edits.
    /// </summary>
    [Route("api/v1/productiondataedit")]
    [HttpGet]
    public ContractExecutionResult GetDataEdit([FromQuery] Guid projectUid, [FromQuery] Guid? assetUid)
    {
      Log.LogInformation($"{nameof(GetDataEdit)}: projectUid={projectUid}, assetUid={assetUid}");

      var result = new TRexEditDataResult(new List<TRexEditData>());
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid);
      if (siteModel == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Project {projectUid} does not exist"));
      }

      if (assetUid.HasValue)
      {
        var machine = siteModel.Machines.Locate(assetUid.Value);
        if (machine == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Asset {assetUid} does not exist in project {projectUid}"));
        }
        GetOverrideEvents(result, siteModel, machine);
      }
      else
      {
        //If assetUid is not provided get overridden events for all machines
        foreach (var machine in siteModel.Machines)
        {
          GetOverrideEvents(result, siteModel, machine);
        }
      }    

      return result;
    }

    /// <summary>
    /// Get the list of overriding events for the given machine
    /// </summary>
    private void GetOverrideEvents(TRexEditDataResult result, ISiteModel siteModel, IMachine machine)
    {
      DateTime startDate;

      var machineTargetValues = siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex];
      var designOverrides = machineTargetValues.DesignOverrideEvents;
      if (designOverrides != null && designOverrides.Count() > 0)
      {
        for (var i = 0; i < designOverrides.Count(); i++)
        {
          designOverrides.GetStateAtIndex(i, out startDate, out OverrideEvent<int> evt);
          var design = siteModel.SiteModelMachineDesigns.Locate(evt.Value);
          result.DataEdits.Add(new TRexEditData(machine.ID, startDate, evt.EndDate, design.Name, null));
        }
      }
      var layerOverrides = machineTargetValues.LayerOverrideEvents;
      if (layerOverrides != null && layerOverrides.Count() > 0)
      {
        for (var i = 0; i < layerOverrides.Count(); i++)
        {
          layerOverrides.GetStateAtIndex(i, out startDate, out OverrideEvent<ushort> evt);
          //Check for matching design override
          var match = result.DataEdits.FirstOrDefault(de => de.AssetUid == machine.ID && de.StartUtc == startDate && de.EndUtc == evt.EndDate);
          if (match != null)
          {
            match.LiftNumber = evt.Value;
          }
          else
          {
            result.DataEdits.Add(new TRexEditData(machine.ID, startDate, evt.EndDate, null, evt.Value));
          }
        }
      }
    }

  }
}
