using System;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Events
{
  public class ProductionEventsFactory : IProductionEventsFactory
  {
    /// <summary>
    /// Create an event list of the requested type
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="eventType"></param>
    /// <param name="machineID"></param>
    /// <returns></returns>
    public IProductionEvents NewEventList(short machineID, Guid siteModelID, ProductionEventType eventType)
    {
      switch (eventType)
      {
        case ProductionEventType.TargetCCV: return new ProductionEvents<short>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadInt16(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.TargetPassCount: return new ProductionEvents<ushort>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadUInt16(), ProductionEventStateEqualityComparer.Equals);

        case ProductionEventType.MachineMapReset: return null; //throw new NotImplementedException("ProductionEventType.MachineMapReset not implemented");

        case ProductionEventType.TargetLiftThickness: return new ProductionEvents<float>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadSingle(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.GPSModeChange: return new ProductionEvents<GPSMode>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (GPSMode)r.ReadByte(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.VibrationStateChange: return new ProductionEvents<VibrationState>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (VibrationState)r.ReadByte(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.AutoVibrationStateChange: return new ProductionEvents<AutoVibrationState>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (AutoVibrationState)r.ReadByte(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.MachineGearChange: return new ProductionEvents<MachineGear>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (MachineGear)r.ReadByte(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.MachineAutomaticsChange: return new ProductionEvents<AutomaticsType>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (AutomaticsType)r.ReadByte(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.MachineRMVJumpValueChange: return new ProductionEvents<short>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadInt16(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.ICFlagsChange: return new ProductionEvents<byte>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadByte(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.ElevationMappingModeStateChange: return new ProductionEvents<ElevationMappingMode>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (ElevationMappingMode)r.ReadByte(), ProductionEventStateEqualityComparer.Equals);

        case ProductionEventType.GPSAccuracyChange:
          return new ProductionEvents<GPSAccuracyAndTolerance>(machineID, siteModelID, eventType,
            (w, s) => { w.Write(s.GPSTolerance); w.Write((byte)s.GPSAccuracy); },
            r => new GPSAccuracyAndTolerance((GPSAccuracy)r.ReadByte(), r.ReadUInt16()),
            ProductionEventStateEqualityComparer.Equals);

        case ProductionEventType.PositioningTech: return new ProductionEvents<PositioningTech>(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (PositioningTech)r.ReadByte(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.TempWarningLevelMinChange: return new ProductionEvents<ushort>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadUInt16(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.TempWarningLevelMaxChange: return new ProductionEvents<ushort>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadUInt16(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.TargetMDP: return new ProductionEvents<short>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadInt16(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.LayerID: return new ProductionEvents<ushort>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadUInt16(), ProductionEventStateEqualityComparer.Equals);

        case ProductionEventType.DesignOverride: return null; //throw new NotImplementedException("ProductionEventType.DesignOverride not implemented");
        case ProductionEventType.LayerOverride: return null; // throw new NotImplementedException("ProductionEventType.LayerOverride not implemented");

        case ProductionEventType.TargetCCA: return new ProductionEvents<byte>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadByte(), ProductionEventStateEqualityComparer.Equals);
        case ProductionEventType.StartEndRecordedData: return new StartEndProductionEvents(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (ProductionEventType)r.ReadByte());
        case ProductionEventType.MachineStartupShutdown: return new StartEndProductionEvents(machineID, siteModelID, eventType, (w, s) => w.Write((byte)s), r => (ProductionEventType)r.ReadByte());

        case ProductionEventType.DesignChange: return new ProductionEvents<int>(machineID, siteModelID, eventType, (w, s) => w.Write(s), r => r.ReadInt32(), ProductionEventStateEqualityComparer.Equals);
        default: return null;
      }

    }
  }
}
