using System;
using System.Collections.Generic;
using VSS.VisionLink.Raptor.Machines.Interfaces;

namespace VSS.VisionLink.Raptor.Machines
{
    /// <summary>
    /// Implements a container for all the machines that have had activity within a Site Model
    /// </summary>
    [Serializable]
    public class MachinesList : List<Machine>, IMachinesList
    {
        /// <summary>
        /// Maps machine IDs (currently as 64 bit integers) to the instance containing all the event lists for all the machines
        /// that have contributed to the owner SiteModel
        /// </summary>
        private Dictionary<long, Machine> MachineIDMap = new Dictionary<long, Machine>();

        /// <summary>
        /// The identifier of the site model owning this list of machines
        /// </summary>
        public long DataModelID { get; set; } = -1;

        public MachinesList(long datamodeID)
        {
            DataModelID = datamodeID;
        }

        /// <summary>
        /// Determine the next unique JohnDoe machine ID to use for a new John Doe machine
        /// </summary>
        /// <returns></returns>
        private long UniqueJohnDoeID()
        {
            long Result = RaptorConfig.JohnDoeBaseNumber() + 1;

            ForEach(x => { if (x.IsJohnDoeMachine && x.ID != Machine.kJohnDoeAssetID && x.ID >= Result) { Result++; } });

            return Result;
        }

        public Machine CreateNew(string name, string machineHardwareID,
                               byte machineType,
                               int deviceType,
                               bool isJohnDoeMachine,
                               long machineID)
        {
            Machine ExistingMachine = isJohnDoeMachine ? Locate(name, true) : Locate(machineID, false);

            if (ExistingMachine != null)
            {
                return ExistingMachine;
            }

            // Create the new machine
            if (isJohnDoeMachine)
            {
                machineID = UniqueJohnDoeID();
            }

            Machine Result = new Machine(this, name, machineHardwareID, machineType, deviceType, machineID, isJohnDoeMachine /* TODO, kICUnknownConnectedMachineLevel*/);

            // Add it to the list
            Add(Result);

            return Result;
        }

        /// <summary>
        /// Overrides the base List T Add() method to add the item to the local machine ID map dictionary as well as add it to the list
        /// </summary>
        /// <param name="machine"></param>
        public new void Add(Machine machine)
        {
            base.Add(machine);

            MachineIDMap.Add(machine.ID, machine);
        }

        /// <summary>
        /// Finds the machine in the list whose name matches the given name
        /// It returns NIL if there is no matching machine
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isJohnDoeMachine"></param>
        /// <returns></returns>
        public Machine Locate(string name, bool isJohnDoeMachine) => Find(x => x.IsJohnDoeMachine == isJohnDoeMachine && name.Equals(x.Name));

        /// <summary>
        // Locate finds the machine in the list whose name matches the given ID
        // It returns NIL if there is no matching machine
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isJohnDoeMachine"></param>
        /// <returns></returns>
        public Machine Locate(long id, bool isJohnDoeMachine) => Find(x => x.IsJohnDoeMachine == isJohnDoeMachine && id == x.ID);

        /// <summary>
        /// Locate finds a machine given the ID of a machine in the list.
        /// It returns NIL if there is no matching machine
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Machine Locate(long id) => MachineIDMap[id];

        // LocateByMachineHardwareID locates the (first) machine in the machines
        // list that has a matching machine hardware ID to the <AID> parameter.
        public Machine LocateByMachineHardwareID(string hardwareID) => Find(x => x.MachineHardwareID.Equals(hardwareID));

    }
}
