using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Models.ResultHandling
{
public class CellPassesV2Result : ContractExecutionResult
  {
    /// <summary>
    /// Contains the information relating to a cell pass. All measurements are made at the center of the cell
    /// </summary>
    public class CellPassValue
    {
      /// <summary>
      /// The measured amplitude of the compaction drum vibration
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public ushort Amplitude { get; set; }

      /// <summary>
      /// The CCV measured by the machine. Value is expressed in 10ths of units
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short Ccv { get; set; }

      /// <summary>
      /// The measured frequency of the compaction drum vibration. The value is expressed in 100ths of milliters
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public ushort Frequency { get; set; }

      /// <summary>
      /// The elevation of the cell pass with respect to datum of the grid coordinate system. The value is expressed in Meters.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public float Height { get; set; }

      /// <summary>
      /// The numeric identifier assigned to the machine at the time the TAG file containing the cell pass is processed
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public long MachineId { get; set; }

      /// <summary>
      /// The calculated speed of the machine at the time the cell pass is measured, measured in centimeters per second.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public ushort MachineSpeed { get; set; }

      /// <summary>
      /// The temperature of the asphalt mat at the time the asphalt compactor rolled over it.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter), 4096)]
      public ushort MaterialTemperature { get; set; }

      /// <summary>
      /// The MDP measured by the machine. Value is expressed in 10ths of units
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short Mdp { get; set; }

      /// <summary>
      /// The radio latency measured at the time the cell was passed over by the machine. Expressed in seconds since the RTK correction was emitted from the base station.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public byte RadioLatency { get; set; }

      /// <summary>
      /// The Resonanace Meter Value measured by a compaction machine when the vibratory rolled over the cell.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short Rmv { get; set; }

      /// <summary>
      /// The time at which the cell was rolled over in this pass. For GPS equipped systems this is GPS time. For ATS/UTS equiped systems this is the GCS internal system clock time. 
      /// The time is expressed un UTC.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public DateTime Time { get; set; }

      /// <summary>
      /// A bit field stored used to hold the GPSMode recorded by the machine at the time the cell was passed over. The LSB four bits in the byte are used for this purpose.
      /// </summary>
      public byte GpsModeStore { get; set; }
    }

    /// <summary>
    /// The collection of target values set at the time a cell pass is recorded.
    /// </summary>
    public class CellTargetsValue
    {
      /// <summary>
      /// The target CCV. In 10ths of units.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short TargetCcv { get; set; }

      /// <summary>
      /// The target MDP. In 10ths of units.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short TargetMdp { get; set; }

      /// <summary>
      /// The target pass count to attain before material is considered to be compacted sufficiently.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public ushort TargetPassCount { get; set; }

      /// <summary>
      /// Target lift thickness for each layer. Value is expressed in meters.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter), 3.4e+38)]
      public float TargetThickness { get; set; }

      /// <summary>
      /// The upper bound of the asphalt temperature mat when being rolled. Values above this cause a warning to be issued on the machine. Expressed in degrees Celcius.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter), 4096)]
      public ushort TempWarningLevelMax { get; set; }

      /// <summary>
      /// The lower bound of the asphalt temperature mat when being rolled. Values below this cause a warning to be issued on the machine. Expressed in degrees Celcius.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter), 4096)]
      public ushort TempWarningLevelMin { get; set; }
    }

    /// <summary>
    /// The values of temporal event information as at the time the cell pass was recorded.
    /// </summary>
    public class CellEventsValue
    {
      /// <summary>
      /// Is the compactor using automatic vibration control
      /// </summary>
      public AutoStateType EventAutoVibrationState { get; set; }

      /// <summary>
      /// The ID of the design loaded machine. This is a foreign key into the design collection maintained in the project.
      /// </summary>
      public int EventDesignNameId { get; set; }

      /// <summary>
      /// The intelligent compaction flags set emitted by the machine. Structure and meaning is dependent on the compaction sensor type installed on the compaction machine.
      /// </summary>
      public byte EventIcFlags { get; set; }

      /// <summary>
      /// The GCS automatics control mode - manual (indicate only) or automatics (blade control)
      /// </summary>
      public GCSAutomaticsModeType EventMachineAutomatics { get; set; }

      /// <summary>
      /// The gear the machine is in
      /// </summary>
      public MachineGearType EventMachineGear { get; set; }

      /// <summary>
      /// The RMV threshold reported by the machine. Values above this level are interpreted as decoupled (the compactor drum is bouncing)
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short EventMachineRmvThreshold { get; set; }

      /// <summary>
      /// Is the machine implemnent (drum, blade, bucket etc) in contact with the ground when the measurement is made. If not decoupled, a compactor drum is considered to be on the groudn by definition.
      /// </summary>
      public OnGroundStateType EventOnGroundState { get; set; }

      /// <summary>
      /// Is the compactor drum in a vibratory state?
      /// </summary>
      public VibrationStateType EventVibrationState { get; set; }

      /// <summary>
      /// The GSP accuracy mode the GCS system is operating under. Fine, medium and coarse relate to accuracy ranges which may be millimeters at the Fine end, and meters are the coarse end.
      /// </summary>
      public GPSAccuracyType GpsAccuracy { get; set; }

      /// <summary>
      /// The GPS tolerance, or error limit, of GPS positions being used. This is a value in the range 0..2^14-1 millimeters.
      /// </summary>
      public ushort GpsTolerance { get; set; }

      /// <summary>
      /// The layer number entered by the operator on the machine.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public ushort LayerId { get; set; }

      /// <summary>
      /// The ID of the design loaded at the time the user executed a map reset on the machine. This is a foreign key into the design collection maintained in the project.
      /// </summary>
      public int MapResetDesignNameId { get; set; }

      /// <summary>
      /// The date of the most recent map reset event prior to the time the cell pass is recorded.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public DateTime MapResetPriorDate { get; set; }

      /// <summary>
      /// The positioning technology used to collect the implement (drum, blade etc) position. The current values are GPS or UTS (Universal Total Station)
      /// </summary>
      public PositioningTechType PositioningTech { get; set; }

      /// <summary>
      /// A set of bit flags that indicate avoidance zone transgression states. Not currently implemented. Documented elsewhere.
      /// </summary>
      public byte EventInAvoidZoneState { get; set; }

      /// <summary>
      /// The GCS machine control system is mapping minimum elevations; typically used by HEX (Hydraulic Excavator machines)
      /// 0 - mmLatestElevation,
      /// 1 - mmMinimumElevation,
      /// 2 - mmMaximumElevation.
      /// </summary>
      /// 
      public byte EventMinElevMapping;
    }

    /// <summary>
    /// The contains all the cell pass, event and target value information releveant to the cell pass at the time is was recorded.
    /// </summary>
    public class FilteredPassData
    {
      /// <summary>
      /// Attributes values for the cell pass
      /// </summary>
      public CellPassValue FilteredPass { get; set; }

      /// <summary>
      /// Values of temporal event information for the cell pass
      /// </summary>
      public CellEventsValue EventsValue { get; set; }

      /// <summary>
      /// Values of attribute target values for the cell pass
      /// </summary>
      public CellTargetsValue TargetsValue { get; set; }
    }

    /// <summary>
    /// The collection of information that describe a material layer, as defined by the collection of cell passes that comprise it.
    /// Values are revelant to the layer as a whole. In the case of cell pass attributes, event or target information they represent the latest known values
    /// for those items as at the time of the last contributory cell pass in the layer that contained a known-value for the attribute in question.
    /// </summary>
    public class ProfileLayer
    {
      /// <summary>
      /// Compaction vibratory drum amplitude. Value is expressed in 100ths of millimeters.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public ushort Amplitude { get; set; }

      /// <summary>
      /// CCV value for the layer. Expressed in 10ths of units.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short Ccv { get; set; }

      /// <summary>
      /// Elevation of the cell pass that contributed the layer CCV value
      /// </summary>
      public float CcvElev { get; set; }

      /// <summary>
      /// The ID of the machine that recorded the cell pass that contributed the CCV value
      /// </summary>
      public long CcvMachineId { get; set; }

      /// <summary>
      /// The time the cell pass was recorded that contributed the CCV value.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public DateTime CcvTime  { get; set; }

      /// <summary>
      /// The number of 'half passes' counted in the layer. A pass made by certain machine types (eg: CTCT GCS Quattro four wheel landfill sheepsfoot compactor) are counted as half passes 
      /// (each axle contributes half of the compactive effort attributed to the machine. All other machine types contribute 'whole passes' (such as drum soil compactors).
      /// </summary>
      public int FilteredHalfPassCount { get; set; }

      /// <summary>
      /// The rounded whole pass count for the layer. Residual half pass counts are rouned up (eg: 3 half passes is rounded up to 2 whole passes)
      /// </summary>
      public int FilteredPassCount { get; set; }

      /// <summary>
      /// The elevation of the first cell pass recorded in the layer.
      /// </summary>
      public float FirstPassHeight { get; set; }

      /// <summary>
      /// The vibratory drum frequency. Value is expressed in 10ths of Hertz.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public ushort Frequency { get; set; }

      /// <summary>
      /// The measured elevation of the last cell pass made in the layer. This represents the best known elevation of the top of the material layer at the location of the cell.
      /// </summary>
      public float Height { get; set; }

      /// <summary>
      /// The time at which the last cell pass ccntributed to this layer was recorded.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public DateTime LastLayerPassTime { get; set; }

      /// <summary>
      /// The elevation of the last cell pass contributed to the layer.
      /// </summary>
      public float LastPassHeight { get; set; }

      /// <summary>
      /// The machine ID of the last cell pass contributed to the layer
      /// </summary>
      public long MachineId { get; set; }

      /// <summary>
      /// The material temperature (recorded by asphalt compactors).
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter), 4096)]
      public ushort MaterialTemperature { get; set; }

      /// <summary>
      /// Elevation of the cell pass that recorded the material temperature for the layer
      /// </summary>
      public float MaterialTemperatureElev { get; set; }

      /// <summary>
      /// The ID of the machine that recorded the material temperature for the layer
      /// </summary>
      public long MaterialTemperatureMachineId { get; set; }

      /// <summary>
      /// The time the cell pass that contributed the material temperature value
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public DateTime MaterialTemperatureTime { get; set; }

      /// <summary>
      /// The maximum elevation recorded across all cell passes contributed to the layer.
      /// </summary>
      public float MaximumPassHeight { get; set; }

      /// <summary>
      /// The maximum layer thickness recorded across all cell passes contributed to the layer.
      /// </summary>
      public float MaxThickness { get; set; }

      /// <summary>
      /// MDP value for the layer. Epressed in 10ths of units.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short Mdp { get; set; }

      /// <summary>
      /// Elevation of the cell pass that contributed the layer MDP value
      /// </summary>
      public float MdpElev { get; set; }

      /// <summary>
      /// The ID of the machine that recorded the cell pass that contributed the MDP value
      /// </summary>
      public long MdpMachineId { get; set; }

      /// <summary>
      /// The time the cell pass was recorded that contributed the CCV value.
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public DateTime MdpTime { get; set; }

      /// <summary>
      /// The lowest elevation value recorded across all cell passes contributed to the layer.
      /// </summary>
      public float MinimumPassHeight { get; set; }

      /// <summary>
      /// The radio latency for the layer
      /// </summary>
      public byte RadioLatency { get; set; }

      /// <summary>
      /// The resonance meter value for the layer
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short Rmv { get; set; }

      /// <summary>
      /// The target CCV value used for the layer
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short TargetCcv { get; set; }

      /// <summary>
      /// The target MDP value used for the layer
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public short TargetMdp { get; set; }

      /// <summary>
      /// The target pass count value used for the layer
      /// </summary>
      public int TargetPassCount { get; set; }

      /// <summary>
      /// The target layer thickness value used for the layer
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter), 3.4e+38)]
      public float TargetThickness { get; set; }

      /// <summary>
      /// The final recorded thickness for the layer once all cell passes have been contributed to it
      /// </summary>
      [JsonConverter(typeof(RaptorNullableConverter))]
      public float Thickness { get; set; }

      /// <summary>
      /// The collection of filtered cell pass data that comprise this cell. This includes cell pass attributes, target value information and temporal event information for each cell pass.
      /// </summary>
      public FilteredPassData[] PassData { get; set; }
    }

    /// <summary>
    /// THe set of layers the comprise this cell. Each layer comprises a unique set of cell passes from the filtered pass data and the information calculated from them.
    /// </summary>
    public ProfileLayer[] Layers { get; set; }

    public CellPassesV2Result()
    {
      
    }

    public CellPassesV2Result(int code) : base(code)
    {
      
    }
  }
}
