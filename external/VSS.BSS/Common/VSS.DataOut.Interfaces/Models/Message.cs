using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.MaintenanceMode;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.RapidReporting;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.Site;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions;
using VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.SwitchConfiguration;

namespace VSS.Nighthawk.DataOut.Interfaces.Models
{
  [Serializable]
  public class Message
  {
    public Message()
    {
      Blocks = new List<Block>();
    }

    public Header Header { get; set; }

    /// <remarks>One to many data blocks.</remarks>
    /// Derived classes from 'Block' class should be added here for auto serialization-de-serialization by the serializer.
    /// NOTE: I needed to add the   [XmlArrayItem(Type = typeof(Block))] attribute in order for the polymorphism to work in my UnitTests: Dblair
    [XmlArrayItem(Type = typeof(AssetName))]
    [XmlArrayItem(Type = typeof(DigitalSwitchConfiguration))]
    [XmlArrayItem(Type = typeof(DiscreteInputConfiguration))]
    [XmlArrayItem(Type = typeof(LocationStatusUpdate))]
    [XmlArrayItem(Type = typeof(MovementDurationSeconds))]
    [XmlArrayItem(Type = typeof(MovementRadiusKilometers))]
    [XmlArrayItem(Type = typeof(MovementSpeedKilometers))]
    [XmlArrayItem(Type = typeof(OdometerKilometersAdjustment))]
    [XmlArrayItem(Type = typeof(PolygonSiteConfiguration))]
    [XmlArrayItem(Type = typeof(RemovePolygonSite))]
    [XmlArrayItem(Type = typeof(ReportStartTimeUtc))]
    [XmlArrayItem(Type = typeof(RuntimeHoursAdjustment))]
    [XmlArrayItem(Type = typeof(EnableMaintenanceMode))]
    [XmlArrayItem(Type = typeof(DisableMaintenanceMode))]
    [XmlArrayItem(Type = typeof(StartModeConfiguration))]
    [XmlArrayItem(Type = typeof(StartModeStatusUpdate))]    
    [XmlArrayItem(Type = typeof(TamperLevelConfiguration))]
    [XmlArrayItem(Type = typeof(TamperLevelStatusUpdate))]
    [XmlArrayItem(Type = typeof(DailyReportFrequency))]
    [XmlArrayItem(Type = typeof(EnableRapidReporting))]
    [XmlArrayItem(Type = typeof(DisableRapidReporting))]
    [XmlArrayItem(Type = typeof(ReportingFrequency))]
    // The following directive exists for testing blocks outside the publicly exposed interface
    [XmlArrayItem(Type = typeof(Util.OutOfBandTestBlock))] 
    [XmlArrayItem(Type = typeof(Block))]
    public List<Block> Blocks { get; set; }
  }
}