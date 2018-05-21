using System;
using System.Collections.Generic;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels;

namespace VSS.TRex.Events
{
    /// <summary>
    /// Stores the list of sets of machine events. Each machine has a set of event lists. MachinesTargetValues 
    /// contains a list of these sets of event lists for multiple machines
    /// </summary>
    public class MachinesProductionEventLists : List<ProductionEventLists>, IMachinesProductionEventLists
    {
        /// <summary>
        /// The Sitemodel instance that owns this set of machines target values
        /// </summary>
        private SiteModel Owner;

        /// <summary>
        /// Maps machine IDs (currently as 16 bit integers) to the instance containing all the event lists for all the machines
        /// that have contributed to the owner SiteModel
        /// </summary>
        private Dictionary<short, ProductionEventLists> MachineIDMap = new Dictionary<short, ProductionEventLists>();

        /// <summary>
        /// Constructor for the machines events within the sitemodel supplier as owner
        /// </summary>
        /// <param name="owner"></param>
        public MachinesProductionEventLists(SiteModel owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// Default (guid) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
        /// events lists for that machine.
        /// </summary>
        /// <param name="MachineID"></param>
        /// <returns></returns>
        //public ProductionEventLists this[Guid MachineID] => MachineIDMap.TryGetValue(MachineID, out ProductionEventLists result) ? result : null;

        /// <summary>
        /// Default (short) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
        /// events lists for that machine.
        /// </summary>
        /// <param name="MachineID"></param>
        /// <returns></returns>
        public ProductionEventLists this[short MachineID] => MachineIDMap.TryGetValue(MachineID, out ProductionEventLists result) ? result : null;

        /// <summary>
        /// Overrides the base List T Add() method to add the item to the local machine ID map dictionary as well as add it to the list
        /// </summary>
        /// <param name="events"></param>
        public new void Add(ProductionEventLists events)
        {
            base.Add(events);

            MachineIDMap.Add(events.MachineID, events);
        }
    }
}
