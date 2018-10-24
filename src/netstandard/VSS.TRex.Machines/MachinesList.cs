using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities.ExtensionMethods;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.Machines
{
  /// <summary>
  /// Implements a container for all the machines that have had activity within a Site Model
  /// </summary>
  public class MachinesList : List<IMachine>, IMachinesList, IBinaryReaderWriter
  {
    private const string kMachinesListStreamName = "Machines";

    /// <summary>
    /// Maps machine IDs (currently as 64 bit integers) to the instance containing all the event lists for all the machines
    /// that have contributed to the owner SiteModel
    /// </summary>
    private Dictionary<Guid, IMachine> MachineIDMap = new Dictionary<Guid, IMachine>();

    /// <summary>
    /// The identifier of the site model owning this list of machines
    /// </summary>
    public Guid DataModelID { get; set; }

    public MachinesList()
    {
    }

    /// <summary>
    /// Determine the next unique JohnDoe machine ID to use for a new John Doe machine
    /// </summary>
    /// <returns></returns>
    private Guid UniqueJohnDoeID() => Guid.NewGuid();

    public IMachine CreateNew(string name, string machineHardwareID,
      byte machineType,
      int deviceType,
      bool isJohnDoeMachine,
      Guid machineID)
    {
      IMachine ExistingMachine = isJohnDoeMachine ? Locate(name, true) : Locate(machineID, false);

      if (ExistingMachine != null)
      {
        return ExistingMachine;
      }

      // Create the new machine
      if (isJohnDoeMachine)
      {
        machineID = UniqueJohnDoeID();
      }

      // Determine the internal ID for the new machine.
      // Note: This assumes machines are never removed from a project

      short internalMachineID = (short) Count;

      Machine Result = new Machine(this, name, machineHardwareID, machineType, deviceType, machineID, internalMachineID, isJohnDoeMachine
        /* TODO, kICUnknownConnectedMachineLevel*/);

      // Add it to the list
      Add(Result);

      return Result;
    }

    /// <summary>
    /// Overrides the base List T Add() method to add the item to the local machine ID map dictionary as well as add it to the list
    /// </summary>
    /// <param name="machine"></param>
    public new void Add(IMachine machine)
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
    public IMachine Locate(string name, bool isJohnDoeMachine) => Find(x => x.IsJohnDoeMachine == isJohnDoeMachine && name.Equals(x.Name));

    /// <summary>
    // Locate finds the machine in the list whose name matches the given ID
    // It returns NIL if there is no matching machine
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isJohnDoeMachine"></param>
    /// <returns></returns>
    public IMachine Locate(Guid id, bool isJohnDoeMachine) => Find(x => x.IsJohnDoeMachine == isJohnDoeMachine && id == x.ID);

    /// <summary>
    /// Locate finds a machine given the ID of a machine in the list.
    /// It returns NIL if there is no matching machine
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IMachine Locate(Guid id) => MachineIDMap.TryGetValue(id, out IMachine result) ? result : null;

    // LocateByMachineHardwareID locates the (first) machine in the machines
    // list that has a matching machine hardware ID to the <AID> parameter.
    public IMachine LocateByMachineHardwareID(string hardwareID) => Find(x => x.MachineHardwareID.Equals(hardwareID));

    /// <summary>
    /// Serialise the list of machine using the given writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(UtilitiesConsts.ReaderWriterVersion);

      writer.Write((int) Count);
      for (int i = 0; i < Count; i++)
        this[i].Write(writer);
    }

    public void Write(BinaryWriter writer, byte[] buffer)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Deserialises the list of machines using the given reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      int version = reader.ReadInt32();
      if (version != UtilitiesConsts.ReaderWriterVersion)
        throw new Exception($"Invalid version number ({version}) reading machines list, expected version (1)");

      int count = reader.ReadInt32();
      Capacity = count;

      for (int i = 0; i < count; i++)
      {
        Machine Machine = new Machine();
        Machine.Read(reader);
        Add(Machine);
      }
    }

    /// <summary>
    /// Saves the content of the machines list into the persistent store
    /// Note: It uses a storage proxy delegate to support the TAG file ingest pipeline that creates transactional storage
    /// proxies to manage graceful rollback of changes if needed
    /// </summary>
    public void SaveToPersistentStore(IStorageProxy storageProxy)
    {
      storageProxy.WriteStreamToPersistentStore(DataModelID, kMachinesListStreamName, FileSystemStreamType.Machines, this.ToStream(), this);
    }

    /// <summary>
    /// Loads the content of the machines list from the tpersistent store. If there is no item in the persistent store containing
    /// machines for this sitemodel them return an empty list.
    /// </summary>
    public void LoadFromPersistentStore()
    {
      DIContext.Obtain<ISiteModels>().StorageProxy.ReadStreamFromPersistentStore(DataModelID, kMachinesListStreamName, FileSystemStreamType.Machines, out MemoryStream MS);
      if (MS == null)
        return;

      using (MS)
      {
        this.FromStream(MS);
      }
    }
  }
}
