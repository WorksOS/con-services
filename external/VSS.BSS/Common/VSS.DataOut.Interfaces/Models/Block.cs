using System;
using System.Runtime.Serialization;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.MaintenanceMode;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.RapidReporting;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.Site;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.SwitchConfiguration;

namespace VSS.Nighthawk.DataOut.Interfaces.Models
{
  [Serializable]
  [KnownType(typeof(AssetName))]
  [KnownType(typeof(DigitalSwitchConfiguration))]
  [KnownType(typeof(DiscreteInputConfiguration))]
  [KnownType(typeof(LocationStatusUpdate))]
  [KnownType(typeof(MovementDurationSeconds))]
  [KnownType(typeof(MovementRadiusKilometers))]
  [KnownType(typeof(MovementSpeedKilometers))]
  [KnownType(typeof(OdometerKilometersAdjustment))]
  [KnownType(typeof(PolygonSiteConfiguration))]
  [KnownType(typeof(RemovePolygonSite))]
  [KnownType(typeof(ReportStartTimeUtc))]
  [KnownType(typeof(RuntimeHoursAdjustment))]
  [KnownType(typeof(DisableMaintenanceMode))]
  [KnownType(typeof(EnableMaintenanceMode))]
  [KnownType(typeof(StartModeConfiguration))]
  [KnownType(typeof(StartModeStatusUpdate))]
  [KnownType(typeof(TamperLevelConfiguration))]
  [KnownType(typeof(TamperLevelStatusUpdate))]
  [KnownType(typeof(DailyReportFrequency))]
  [KnownType(typeof(EnableRapidReporting))]
  [KnownType(typeof(DisableRapidReporting))]
  [KnownType(typeof(ReportingFrequency))]
  [KnownType(typeof(Util.OutOfBandTestBlock))]
  public class Block
  { }
}
