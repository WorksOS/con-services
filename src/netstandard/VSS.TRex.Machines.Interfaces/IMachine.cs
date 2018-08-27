using System;
using System.IO;
using VSS.TRex.Types;
using VSS.TRex.Events.Interfaces;

namespace VSS.TRex.Machines.Interfaces
{
  public interface IMachine
  {
    Guid ID { get; set; }
    short InternalSiteModelMachineIndex { get; set; }
    string Name { get; set; }
    byte MachineType { get; set; }
    int DeviceType { get; set; }
    string MachineHardwareID { get; set; }
    bool IsJohnDoeMachine { get; set; }
    double LastKnownX { get; set; }
    double LastKnownY { get; set; }
    DateTime LastKnownPositionTimeStamp { get; set; }
    string LastKnownDesignName { get; set; }
    ushort LastKnownLayerId { get; set; }

    IProductionEventLists TargetValueChanges { get; }

    /// <summary>
    /// Indicates if the machine has ever reported any compactrion realated data, such as CCV, MDP or CCA measurements
    /// </summary>
    bool CompactionDataReported { get; set; }

    CompactionSensorType CompactionSensorType { get; set; }

    /// <summary>
    /// Determines if the type of this machine is one of the machine tyeps that supports compaction operations
    /// </summary>
    /// <returns></returns>
    bool MachineIsConpactorType();

    void Assign(IMachine source);

    /// <summary>
    /// Serialises machine using the given writer
    /// </summary>
    /// <param name="writer"></param>
    void Write(BinaryWriter writer);

    /// <summary>
    /// Deserialises the machine using the given reader
    /// </summary>
    /// <param name="reader"></param>
    void Read(BinaryReader reader);
  }
}
