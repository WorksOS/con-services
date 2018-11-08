using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/events")]
  public class EventsController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<EventsController>();

    /// <summary>
    /// Returns the list of available event types
    /// </summary>
    /// <returns></returns>
    [HttpGet("types")]
    public JsonResult GetEventTypes()
    {
      return new JsonResult(new List<(ProductionEventType EventType, string Name)>
      {
        (ProductionEventType.MachineStartupShutdown, "Startup/Shutdown"),
        (ProductionEventType.StartEndRecordedData, "Start/End"),
        (ProductionEventType.DesignChange, "Design Change"),
        (ProductionEventType.TargetCCV, "Target CCV"),
        (ProductionEventType.TargetMDP, "Target MDP"),
        (ProductionEventType.TargetCCA, "Target CCA"),
        (ProductionEventType.TargetPassCount, "Target Pass Count"),
        (ProductionEventType.MachineMapReset, "Machine Map Reset"),
        (ProductionEventType.TargetLiftThickness, "Target Lift Thickness"),
        (ProductionEventType.GPSModeChange, "GPS Mode"),
        (ProductionEventType.VibrationStateChange, "Vibe State"),
        (ProductionEventType.AutoVibrationStateChange, "Auto Vibe State"),
        (ProductionEventType.MachineGearChange, "Machine Gear"),
        (ProductionEventType.MachineAutomaticsChange, "Machine Automatics"),
        (ProductionEventType.MachineRMVJumpValueChange, "RMV Jump"),
        (ProductionEventType.ICFlagsChange, "IC Flags"),
        (ProductionEventType.MinElevMappingStateChange, "Min Elevation Mapping"),
        (ProductionEventType.GPSAccuracyChange, "GPS Accuracy"),
        (ProductionEventType.PositioningTech, "Positioning Tech"),
        (ProductionEventType.TempWarningLevelMinChange, "Min Temp Warning Level"),
        (ProductionEventType.TempWarningLevelMaxChange, "Max Temp Warning Level"),
        (ProductionEventType.LayerID, "Layer ID"),
        (ProductionEventType.DesignOverride, "Design Override"),
        (ProductionEventType.LayerOverride, "Layer Override")
      });
    }

    /// <summary>
    /// Gets a list of events of a particular type for a machine in a project. Each event is represented as a
    /// human readable text string. The range of queries is restricted by the date range, and the maximum
    /// number of events in the request
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="machineID"></param>
    /// <param name="eventType"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="maxEventsToReturn"></param>
    /// <returns></returns>
    [HttpGet("text/{siteModelID}/{machineID}/{eventType}")]
    public JsonResult GetEventListAsText(string siteModelID, 
      string machineID,
      int eventType,
      [FromQuery] DateTime startDate,
      [FromQuery] DateTime endDate,
      [FromQuery] int maxEventsToReturn)
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID));
      IMachine machine = siteModel?.Machines.Locate(Guid.Parse(machineID));

      if (siteModel == null || machine == null)
        return new JsonResult($"Sitemodel {siteModelID} and/or machine {machineID} unknown");

      return new JsonResult(siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].GetEventList((ProductionEventType)eventType).ToStrings(startDate, endDate, maxEventsToReturn));
    }
  }
}
