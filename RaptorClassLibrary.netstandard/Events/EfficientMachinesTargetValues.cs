using System.Collections.Generic;
using VSS.VisionLink.Raptor.Machines.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// Stores the list of sets of machine events. Each machine has a set of event lists. MachinesTargetValues 
    /// contains a list of these sets of event lists for multiple machines
    /// </summary>
    public class MachinesProductionEventLists : List<ProductionEventLists>, IMachinesTargetValuesList
    {
        /// <summary>
        /// The Sitemodel instance that owns this set of machines target values
        /// </summary>
        private SiteModel Owner;

        /// <summary>
        /// Maps machine IDs (currently as 64 bit integers) to the instance containing all the event lists for all the machines
        /// that have contributed to the owner SiteModel
        /// </summary>
        private Dictionary<long, ProductionEventLists> MachineIDMap = new Dictionary<long, ProductionEventLists>();

        /// <summary>
        /// Constructor for the machines events within the sitemodel supplier as owner
        /// </summary>
        /// <param name="owner"></param>
        public MachinesProductionEventLists(SiteModel owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// Default (long) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
        /// events lists for that machine.
        /// </summary>
        /// <param name="MachineID"></param>
        /// <returns></returns>
        public ProductionEventLists this[long MachineID] => MachineIDMap.TryGetValue(MachineID, out ProductionEventLists result) ? result : null;

        /// <summary>
        /// Default (int) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
        /// events lists for that machine.
        /// </summary>
        /// <param name="MachineID"></param>
        /// <returns></returns>
        public new ProductionEventLists this[int MachineID] => MachineIDMap.TryGetValue(MachineID, out ProductionEventLists result) ? result : null;

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

/*
    /// <summary>
    /// Stores the list of sets of machine events. Each machine has a set of event lists. MachinesTargetValues 
    /// contains a list of these sets of event lists for multiple machines
    /// </summary>
    public class EfficientMachinesTargetValuesList : List<EfficientProductionEventChanges>, IMachinesTargetValuesList
    {
        /// <summary>
        /// The Sitemodel instance that owns this set of machines target values
        /// </summary>
        private SiteModel Owner;

        /// <summary>
        /// Maps machine IDs (currently as 64 bit integers) to the instance containing all the event lists for all the machines
        /// that have contributed to the owner SiteModel
        /// </summary>
        private Dictionary<long, EfficientProductionEventChanges> MachineIDMap = new Dictionary<long, EfficientProductionEventChanges>();

        /// <summary>
        /// Constructor for the machines events within the sitemodel supplier as owner
        /// </summary>
        /// <param name="owner"></param>
        public EfficientMachinesTargetValuesList(SiteModel owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// Default (long) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
        /// events lists for that machine.
        /// </summary>
        /// <param name="MachineID"></param>
        /// <returns></returns>
        public EfficientProductionEventChanges this[long MachineID] => MachineIDMap.TryGetValue(MachineID, out EfficientProductionEventChanges result) ? result : null;

        /// <summary>
        /// Default (int) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
        /// events lists for that machine.
        /// </summary>
        /// <param name="MachineID"></param>
        /// <returns></returns>
        public new EfficientProductionEventChanges this[int MachineID] => MachineIDMap.TryGetValue(MachineID, out EfficientProductionEventChanges result) ? result : null;

        /// <summary>
        /// Overrides the base List T Add() method to add the item to the local machine ID map dictionary as well as add it to the list
        /// </summary>
        /// <param name="events"></param>
        public new void Add(EfficientProductionEventChanges events)
        {
            base.Add(events);

            MachineIDMap.Add(events.MachineID, events);
        }
    }
*/

}
