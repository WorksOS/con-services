using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Events
{
  /// <summary>
  /// Stores the list of sets of machine events. Each machine has a set of event lists. MachinesTargetValues 
  /// contains a list of these sets of event lists for multiple machines
  /// </summary>
  public class MachinesProductionEventLists : IMachinesProductionEventLists
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<MachinesProductionEventLists>();

    /// <summary>
    /// The Sitemodel instance that owns this set of machines target values
    /// </summary>
    private readonly ISiteModel Owner;

    /// <summary>
    /// Maps machine IDs (currently as 16 bit integers) to the instance containing all the event lists for all the machines
    /// that have contributed to the owner SiteModel
    /// </summary>
    private IProductionEventLists[] MachineIDMap;

    /// <summary>
    /// Constructor for the machines events within the sitemodel supplier as owner
    /// </summary>
    /// <param name="owner"></param>
    public MachinesProductionEventLists(ISiteModel owner)
    {
      Owner = owner;

      MachineIDMap = Enumerable.Range(0, owner.Machines.Count).Select(x => new ProductionEventLists(owner, (short)x) as IProductionEventLists).ToArray();
    }

    /// <summary>
    /// </summary>
    /// <param name="machineID"></param>
    /// <returns></returns>
    private IProductionEventLists GetMachineEventLists(short machineID)
    {
      if (machineID < 0 || machineID >= MachineIDMap.Length)
        return null;

      if (MachineIDMap[machineID] == null)
      {
        Log.LogCritical($"Sitemodel {Owner.ID} asked for non existent machine events at index {machineID}");
        return null;
      }

      return MachineIDMap[machineID];
    }

    /// <summary>
    /// Default (short) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
    /// events lists for that machine.
    /// </summary>
    /// <param name="machineID"></param>
    /// <returns></returns>
    public IProductionEventLists this[short machineID] => GetMachineEventLists(machineID);

    /// <summary>
    /// Overrides the base List T Add() method to add the item to the local machine ID map dictionary as well as add it to the list
    /// </summary>
    /// <param name="events"></param>
    public void Add(IProductionEventLists events)
    {
      if (events.MachineID >= MachineIDMap.Length)
        Array.Resize(ref MachineIDMap, events.MachineID + 1);

      MachineIDMap[events.MachineID] = events;
    }
  }
}
