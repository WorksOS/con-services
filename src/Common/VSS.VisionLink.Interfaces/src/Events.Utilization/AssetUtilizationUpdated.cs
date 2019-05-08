using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Interfaces.Events.Utilization
{
  public class AssetUtilizationUpdated
  {
    public string AssetUid { get; set; }

    public int KeyDate { get; set; }
    /// <summary>
    ///   The date of the asset utilization.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///   The asset's work definition type for the date.
    /// </summary>
    public WorkDefinitionTypeEnum WorkDefinitionType { get; set; }

    /// <summary>
    ///   The asset runtime hours for the date.
    /// </summary>
    public double? RuntimeHours { get; set; }

    /// <summary>
    ///   The asset idle hours for the date.
    /// </summary>
    public double? IdleHours { get; set; }

    /// <summary>
    ///   The asset working hours for the date.
    /// </summary>
    public double? WorkingHours { get; set; }

    /// <summary>
    ///   The runtime hours callout type.
    /// </summary>
    public CalloutTypeEnum RuntimeHoursCalloutType { get; set; }

    /// <summary>
    ///   The idle hours callout type.
    /// </summary>
    public CalloutTypeEnum IdleHoursCalloutType { get; set; }

    /// <summary>
    ///   The working hours callout type.
    /// </summary>
    public CalloutTypeEnum WorkingHoursCalloutType { get; set; }

    /// <summary>
    ///   The asset travelled distance in km for the date.
    /// </summary>
    public double? DistanceTravelledKm { get; set; }

    public DateTime ActionUTC { get; set; }

    /// <summary>
    ///  The asset idle efficiency over runtime
    /// </summary>
    public double? idleEfficiency { get; set; }

    /// <summary>
    /// The asset working efficiency over runtime
    /// </summary>
    public double? workingEfficiency { get; set; }

    /// <summary>
    ///  The asset target idle over actual idle
    /// </summary>
    public double? targetIdleEfficiency { get; set; }

    /// <summary>
    /// The asset target runtime over actual runtime
    /// </summary>
    public double? targetRuntimeEfficiency { get; set; }

  }

  public enum WorkDefinitionTypeEnum
  {
    Unknown = 0,
    MovementEvents = 1,
    SwitchEvents = 2,
    MovementAndSwitchEvents = 3,
    MeterDelta = 4
  }

  public enum CalloutTypeEnum
  {
    None = 0,
    MissingMeterValue = 1,
    MultipleDayDelta = 2,
    Spike = 3,
    NotApplicable = 4,
    NegativeValue = 5,
    NoData = 6,
    MissingTotalFuelData = 7
  }

}
