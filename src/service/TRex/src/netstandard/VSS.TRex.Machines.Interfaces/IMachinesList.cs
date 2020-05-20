using System;
using System.Collections.Generic;
using System.IO;
using VSS.MasterData.Models.Models;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Machines.Interfaces
{
  public interface IMachinesList : IList<IMachine>
  {
    /// <summary>
    /// The identifier of the site model owning this list of machines
    /// </summary>
    Guid DataModelID { get; set; }

    /// <summary>
    /// Finds the machine in the list whose name matches the given name
    /// It returns NIL if there is no matching machine
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isJohnDoeMachine"></param>
    /// <returns></returns>
    IMachine Locate(string name, bool isJohnDoeMachine);

    /// <summary>
    /// Locate finds the machine in the list whose name matches the given ID
    /// It returns NIL if there is no matching machine
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isJohnDoeMachine"></param>
    /// <returns></returns>
    IMachine Locate(Guid id, bool isJohnDoeMachine);

    /// <summary>
    /// Locate finds a machine given the ID of a machine in the list.
    /// It returns NIL if there is no matching machine
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IMachine Locate(Guid id);

    /// <summary>
    /// Finds a machine given a machine ID, the machine name and whether the machine is John Doe machine
    /// If a machine matching the given ID is located it is returned, else if the machine is expected to be a John Doe a
    /// machine is attempted to be located using a specific John Doe machine name approach
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="isJohnDoeMachine"></param>
    /// <returns></returns>
    IMachine Locate(Guid id, string name, bool isJohnDoeMachine);

    // LocateByMachineHardwareID locates the (first) machine in the machines
    // list that has a matching machine hardware ID to the <AID> parameter.
    IMachine LocateByMachineHardwareID(string hardwareID);

    /// <summary>
    /// Deserializes the list of machines using the given reader
    /// </summary>
    /// <param name="reader"></param>
    void Read(BinaryReader reader);

    /// <summary>
    /// Serialize the list of machine using the given writer
    /// </summary>
    /// <param name="writer"></param>
    void Write(BinaryWriter writer);

    IMachine CreateNew(string name, string machineHardwareID,
      MachineType machineType,
      DeviceTypeEnum deviceType,
      bool isJohnDoeMachine,
      Guid machineID);

    void SaveToPersistentStore(IStorageProxy storageProxy);
    void RemoveFromPersistentStore(IStorageProxy storageProxy);
    void LoadFromPersistentStore(IStorageProxy storageProxy);
  }
}
