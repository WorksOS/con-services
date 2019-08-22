using System;
using Newtonsoft.Json;
#if RAPTOR
using SVOICDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// The collection of information that describe a cell passes profile, as defined by the collection of cell passes that comprise it.
  /// Values are revelant to the cell as a whole. In the case of cell attributes, event or target information they represent the latest known values
  /// for those items as at the time of the last contributory cell pass in the cell that contained a known-value for the attribute in question.
  /// Composite elevations are elevations that are calculated from a combination of elevation information from production data sourced from TAG files produced
  /// by machine control systems, the elevation information obtained from dated topological surveys (surveyed surfaces).
  /// </summary>  
  public class CellPassesResult : ContractExecutionResult
  {
    public CellPassesResult(string message) : base(message)
    { }

    /// <summary>
    /// Contains the information relating to a cell pass. All measurements are made at the center of the cell
    /// </summary>
    public class CellPassValue
    {
      /// <summary>
      /// The measured amplitude of the compaction drum vibration
      /// </summary>
      [JsonProperty(PropertyName = "amplitude")]
      public ushort Amplitude;

      /// <summary>
      /// The CCV measured by the machine. Value is expressed in 10ths of units
      /// </summary>
      [JsonProperty(PropertyName = "cCV")]
      public short CCV;

      /// <summary>
      /// The measured frequency of the compaction drum vibration. The value is expressed in 100ths of milliters
      /// </summary>
      [JsonProperty(PropertyName = "frequency")]
      public ushort Frequency;

      /// <summary>
      /// The elevation of the cell pass with respect to datum of the grid coordinate sysem. The value is expressed in Meters.
      /// </summary>
      [JsonProperty(PropertyName = "height")]
      public float Height;

      /// <summary>
      /// The numeric identifier assigned to the machine at the time the TAG file containing the cellk pass is processed
      /// </summary>
      [JsonProperty(PropertyName = "machineID")]
      public long MachineID;

      /// <summary>
      /// The calculated speed of the machine at the time the cell pass is measured, measured in centimeters per second.
      /// </summary>
      [JsonProperty(PropertyName = "machineSpeed")]
      public ushort MachineSpeed;

      /// <summary>
      /// The temperature of the asphalt mat at the time the asphalt compactor rolled over it.
      /// </summary>
      [JsonProperty(PropertyName = "materialTemperature")]
      public ushort MaterialTemperature;

      /// <summary>
      /// The MDP measured by the machine. Value is expressed in 10ths of units
      /// </summary>
      [JsonProperty(PropertyName = "mDP")]
      public short MDP;

      /// <summary>
      /// The radio latency measured at the time the cell was passed over by the machine. Expressed in seconds since the RTK correction was emitted from the base station.
      /// </summary>
      [JsonProperty(PropertyName = "radioLatency")]
      public byte RadioLatency;

      /// <summary>
      /// The Resonanace Meter Value measured by a compaction machine when the vibratory rolled over the cell.
      /// </summary>
      [JsonProperty(PropertyName = "rMV")]
      public short RMV;

      /// <summary>
      /// The time at which the cell was rolled over in this pass. For GPS equipped systems this is GPS time. For ATS/UTS equiped systems this is the GCS internal system clock time. 
      /// The time is expressed un UTC.
      /// </summary>
      [JsonProperty(PropertyName = "time")]
      public DateTime Time;

      /// <summary>
      /// A bit field stored used to hold the GPSMode recorded by the machine at the time the cell was passed over. The LSB four bits in the byte are used for this purpose.
      /// </summary>
      [JsonProperty(PropertyName = "gPSModeStore")]
      public byte GPSModeStore;
    }

    /// <summary>
    /// The collection of target values set at the time a cell pass is recorded.
    /// </summary>
    public class CellTargetsValue
    {
      /// <summary>
      /// The target CCV. In 10ths of units.
      /// </summary>
      [JsonProperty(PropertyName = "targetCCV")]
      public short TargetCCV;

      /// <summary>
      /// The target MDP. In 10ths of units.
      /// </summary>
      [JsonProperty(PropertyName = "targetMDP")]
      public short TargetMDP;

      /// <summary>
      /// The target pass count to attain before material is considered to be compacted sufficiently.
      /// </summary>
      [JsonProperty(PropertyName = "targetPassCount")]
      public ushort TargetPassCount;

      /// <summary>
      /// Target lift thickness for each layer. Value is expressed in meters.
      /// </summary>
      [JsonProperty(PropertyName = "targetThickness")]
      public float TargetThickness;

      /// <summary>
      /// The upper bound of the asphalt temperature mat when being rolled. Values above this cause a warning to be issued on the machine. Expressed in degrees Celcius.
      /// </summary>
      [JsonProperty(PropertyName = "tempWarningLevelMax")]
      public ushort TempWarningLevelMax;

      /// <summary>
      /// The lower bound of the asphalt temperature mat when being rolled. Values below this cause a warning to be issued on the machine. Expressed in degrees Celcius.
      /// </summary>
      [JsonProperty(PropertyName = "tempWarningLevelMin")]
      public ushort TempWarningLevelMin;
    }

    /// <summary>
    /// The values of temporal event information as at the time the cell pass was recorded.
    /// </summary>
    public class CellEventsValue
    {
      /// <summary>
      /// Is the compactor using automatic vibration control
      /// </summary>
      [JsonProperty(PropertyName = "eventAutoVibrationState")]
      public AutoStateType EventAutoVibrationState;

      /// <summary>
      /// The ID of the design loaded machine. This is a foreign key into the design collection maintained in the project.
      /// </summary>
      [JsonProperty(PropertyName = "eventDesignNameID")]
      public int EventDesignNameID;

      /// <summary>
      /// The intelligent compaction flags set emitted by the machine. Structure and meaning is dependent on the compaction sensor type installed on the compaction machine.
      /// </summary>
      [JsonProperty(PropertyName = "eventICFlags")]
      public byte EventICFlags;

      /// <summary>
      /// The GCS automatics control mode - manual (indicate only) or automatics (blade control)
      /// </summary>
      [JsonProperty(PropertyName = "eventMachineAutomatics")]
      public GCSAutomaticsModeType EventMachineAutomatics;

      /// <summary>
      /// The gear the machine is in
      /// </summary>
      [JsonProperty(PropertyName = "eventMachineGear")]
      public MachineGearType EventMachineGear;

      /// <summary>
      /// The RMV threshold reported by the machine. Values above this level are interpreted as decoupled (the compactor drum is bouncing)
      /// </summary>
      [JsonProperty(PropertyName = "eventMachineRMVThreshold")]
      public short EventMachineRMVThreshold;

      /// <summary>
      /// Is the machine implemnent (drum, blade, bucket etc) in contact with the ground when the measurement is made. If not decoupled, a compactor drum is considered to be on the groudn by definition.
      /// </summary>
      [JsonProperty(PropertyName = "eventOnGroundState")]
      public OnGroundStateType EventOnGroundState;

      /// <summary>
      /// Is the compactor drum in a vibratory state?
      /// </summary>
      [JsonProperty(PropertyName = "eventVibrationState")]
      public VibrationStateType EventVibrationState;

      /// <summary>
      /// The GSP accuracy mode the GCS system is operating under. Fine, medium and coarse relate to accuracy ranges which may be millimeters at the Fine end, and meters are the coarse end.
      /// </summary>
      [JsonProperty(PropertyName = "gPSAccuracy")]
      public GPSAccuracyType GPSAccuracy;

      /// <summary>
      /// The GPS tolerance, or error limit, of GPS positions being used. This is a value in the range 0..2^14-1 millimeters.
      /// </summary>
      [JsonProperty(PropertyName = "gPSTolerance")]
      public ushort GPSTolerance;

      /// <summary>
      /// The layer number entered by the operator on the machine.
      /// </summary>
      [JsonProperty(PropertyName = "layerID")]
      public ushort LayerID;

      /// <summary>
      /// The ID of the design loaded at the time the user executed a map reset on the machine. This is a foreign key into the design collection maintained in the project.
      /// </summary>
      [JsonProperty(PropertyName = "mapReset_DesignNameID")]
      public int MapReset_DesignNameID;

      /// <summary>
      /// The date of the most recent map reset event prior to the time the cell pass is recorded.
      /// </summary>
      [JsonProperty(PropertyName = "mapReset_PriorDate")]
      public DateTime MapReset_PriorDate;

      /// <summary>
      /// The positioning technology used to collect the implement (drum, blade etc) position. The current values are GPS or UTS (Universal Total Station)
      /// </summary>
      [JsonProperty(PropertyName = "positioningTech")]
      public PositioningTechType PositioningTech;

      /// <summary>
      /// A set of bit flags that indicate avoidance zone transgression states. Not currently implemented. Documented elsewhere.
      /// </summary>
      public byte EventInAvoidZoneState;

      /// <summary>
      /// The GCS machine control system is mapping minimum elevations; typically used by HEX (Hydraulic Excavator machines)
      /// 0 - mmLatestElevation,
      /// 1 - mmMinimumElevation,
      /// 2 - mmMaximumElevation.
      /// </summary>
#if RAPTOR
      public TICMinElevMappingState EventMinElevMapping;
#else
      public byte EventMinElevMapping;
#endif
    }

    /// <summary>
    /// The contains all the cell pass, event and target value information releveant to the cell pass at the time is was recorded.
    /// </summary>
    public class FilteredPassData
    {
      /// <summary>
      /// Attributes values for the cell pass
      /// </summary>
      [JsonProperty(PropertyName = "filteredPass")]
      public CellPassValue FilteredPass;

      /// <summary>
      /// Values of temporal event information for the cell pass
      /// </summary>
      [JsonProperty(PropertyName = "eventsValue")]
      public CellEventsValue EventsValue;

      /// <summary>
      /// Values of attribute target values for the cell pass
      /// </summary>
      [JsonProperty(PropertyName = "targetsValue")]
      public CellTargetsValue TargetsValue;
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
      [JsonProperty(PropertyName = "amplitude")]
      public ushort Amplitude;

      /// <summary>
      /// CCV value for the layer. Expressed in 10ths of units.
      /// </summary>
      [JsonProperty(PropertyName = "cCV")]
      public short CCV;

      /// <summary>
      /// Elevation of the cell pass that contributed the layer CCV value
      /// </summary>
      [JsonProperty(PropertyName = "cCV_Elev")]
      public float CCV_Elev;

      /// <summary>
      /// The ID of the machine that recorded the cell pass that contributed the CCV value
      /// </summary>
      [JsonProperty(PropertyName = "cCV_MachineID")]
      public long CCV_MachineID;

      /// <summary>
      /// The time the cell pass was recorded that contributed the CCV value.
      /// </summary>
      [JsonProperty(PropertyName = "cCV_Time")]
      public DateTime CCV_Time;

      /// <summary>
      /// The number of 'half passes' counted in the layer. A pass made by certain machine types (eg: CTCT GCS Quattro four wheel landfill sheepsfoot compactor) are counted as half passes 
      /// (each axle contributes half of the compactive effort attributed to the machine. All other machine types contribute 'whole passes' (such as drum soil compactors).
      /// </summary>
      [JsonProperty(PropertyName = "filteredHalfPassCount")]
      public int FilteredHalfPassCount;

      /// <summary>
      /// The rounded whole pass count for the layer. Residual half pass counts are rouned up (eg: 3 half passes is rounded up to 2 whole passes)
      /// </summary>
      [JsonProperty(PropertyName = "filteredPassCount")]
      public int FilteredPassCount;

      /// <summary>
      /// The elevation of the first cell pass recorded in the layer.
      /// </summary>
      [JsonProperty(PropertyName = "firstPassHeight")]
      public float FirstPassHeight;

      /// <summary>
      /// The vibratory drum frequency. Value is expressed in 10ths of Hertz.
      /// </summary>
      [JsonProperty(PropertyName = "frequency")]
      public ushort Frequency;

      /// <summary>
      /// The measured elevation of the last cell pass made in the layer. This represents the best known elevation of the top of the material layer at the location of the cell.
      /// </summary>
      [JsonProperty(PropertyName = "height")]
      public float Height;

      /// <summary>
      /// The time at which the last cell pass ccntributed to this layer was recorded.
      /// </summary>
      [JsonProperty(PropertyName = "lastLayerPassTime")]
      public DateTime LastLayerPassTime;

      /// <summary>
      /// The elevation of the last cell pass contributed to the layer.
      /// </summary>
      [JsonProperty(PropertyName = "lastPassHeight")]
      public float LastPassHeight;

      /// <summary>
      /// The machine ID of the last cell pass contributed to the layer
      /// </summary>
      [JsonProperty(PropertyName = "machineID")]
      public long MachineID;

      /// <summary>
      /// The material temperature (recorded by asphalt compactors).
      /// </summary>
      [JsonProperty(PropertyName = "materialTemperature")]
      public ushort MaterialTemperature;

      /// <summary>
      /// Elevation of the cell pass that recorded the material temperature for the layer
      /// </summary>
      [JsonProperty(PropertyName = "materialTemperature_Elev")]
      public float MaterialTemperature_Elev;

      /// <summary>
      /// The ID of the machine that recorded the material temperature for the layer
      /// </summary>
      [JsonProperty(PropertyName = "materialTemperature_MachineID")]
      public long MaterialTemperature_MachineID;

      /// <summary>
      /// The time the cell pass that contributed the material temperature value
      /// </summary>
      [JsonProperty(PropertyName = "materialTemperature_Time")]
      public DateTime MaterialTemperature_Time;

      /// <summary>
      /// The maximum elevation recorded across all cell passes contributed to the layer.
      /// </summary>
      [JsonProperty(PropertyName = "maximumPassHeight")]
      public float MaximumPassHeight;

      /// <summary>
      /// The maximum layer thickness recorded across all cell passes contributed to the layer.
      /// </summary>
      [JsonProperty(PropertyName = "maxThickness")]
      public float MaxThickness;

      /// <summary>
      /// MDP value for the layer. Epressed in 10ths of units.
      /// </summary>
      [JsonProperty(PropertyName = "mDP")]
      public short MDP;

      /// <summary>
      /// Elevation of the cell pass that contributed the layer MDP value
      /// </summary>
      [JsonProperty(PropertyName = "mDP_Elev")]
      public float MDP_Elev;

      /// <summary>
      /// The ID of the machine that recorded the cell pass that contributed the MDP value
      /// </summary>
      [JsonProperty(PropertyName = "mDP_MachineID")]
      public long MDP_MachineID;

      /// <summary>
      /// The time the cell pass was recorded that contributed the CCV value.
      /// </summary>
      [JsonProperty(PropertyName = "mDP_Time")]
      public DateTime MDP_Time;

      /// <summary>
      /// The lowest elevation value recorded across all cell passes contributed to the layer.
      /// </summary>
      [JsonProperty(PropertyName = "minimumPassHeight")]
      public float MinimumPassHeight;

      /// <summary>
      /// The radio latency for the layer
      /// </summary>
      [JsonProperty(PropertyName = "radioLatency")]
      public byte RadioLatency;

      /// <summary>
      /// The resonance meter value for the layer
      /// </summary>
      [JsonProperty(PropertyName = "rMV")]
      public short RMV;

      /// <summary>
      /// The target CCV value used for the layer
      /// </summary>
      [JsonProperty(PropertyName = "targetCCV")]
      public short TargetCCV;

      /// <summary>
      /// The target MDP value used for the layer
      /// </summary>
      [JsonProperty(PropertyName = "targetMDP")]
      public short TargetMDP;

      /// <summary>
      /// The target pass count value used for the layer
      /// </summary>
      [JsonProperty(PropertyName = "targetPassCount")]
      public int TargetPassCount;

      /// <summary>
      /// The target layer thickness value used for the layer
      /// </summary>
      [JsonProperty(PropertyName = "targetThickness")]
      public float TargetThickness;

      /// <summary>
      /// The final recorded thickness for the layer once all cell passes have been contributed to it
      /// </summary>
      [JsonProperty(PropertyName = "thickness")]
      public float Thickness;

      /// <summary>
      /// The collection of filtered cell pass data that comprise this cell. This includes cell pass attributes, target value information and temporal event information for each cell pass.
      /// </summary>
      [JsonProperty(PropertyName = "filteredPassData")]
      public FilteredPassData[] FilteredPassData;
    }

    /// <summary>
    /// The internal result code returned by the service for the request. Values documented elsewhere.
    /// </summary>
    [JsonIgnore]
    public int ReturnCode;

    /// <summary>
    /// CCV value
    /// </summary>
    [JsonIgnore]
    public short CellCCV;

    /// <summary>
    /// Elevation from cell pass that contributed the CCV
    /// </summary>
    [JsonIgnore]
    public float CellCCVElev;

    /// <summary>
    /// First (in time) composite elevation recorded in the cell
    /// </summary>
    [JsonIgnore]
    public float CellFirstCompositeElev;

    /// <summary>
    /// First (in time) production data only elevation recorded in the cell
    /// </summary>
    [JsonIgnore]
    public float CellFirstElev;

    /// <summary>
    /// Highest composite elevation recorded in the cell
    /// </summary>
    [JsonIgnore]
    public float CellHighestCompositeElev;

    /// <summary>
    /// Highest production data elevation recorded in the cell
    /// </summary>
    [JsonIgnore]
    public float CellHighestElev;

    /// <summary>
    /// Last (in time) composite elevation recorded in the cell
    /// </summary>
    [JsonIgnore]
    public float CellLastCompositeElev;

    /// <summary>
    /// Last (in time) production data only elevation recorded in the cell
    /// </summary>
    [JsonIgnore]
    public float CellLastElev;

    /// <summary>
    /// Lowest composite elevation recorded in the cell
    /// </summary>
    [JsonIgnore]
    public float CellLowestCompositeElev;

    /// <summary>
    /// Lowest production data elevation recorded in the cell
    /// </summary>
    [JsonIgnore]
    public float CellLowestElev;

    /// <summary>
    /// Material temperature for the cell
    /// </summary>
    [JsonIgnore]
    public ushort CellMaterialTemperature;

    /// <summary>
    /// Elevation of the cell pass that recorded the material temperature for the cell
    /// </summary>
    [JsonIgnore]
    public float CellMaterialTemperatureElev;

    /// <summary>
    /// Maximum material temperature warning for the cell
    /// </summary>
    [JsonIgnore]
    public ushort CellMaterialTemperatureWarnMax;

    /// <summary>
    /// Minimum material temperature warning for the cell
    /// </summary>
    [JsonIgnore]
    public ushort CellMaterialTemperatureWarnMin;

    /// <summary>
    /// Total filtered half pass count for the cell
    /// A pass made by certain machine types (eg: CTCT GCS Quattro four wheel landfill sheepsfoot compactor) are counted as half passes 
    /// (each axle contributes half of the compactive effort attributed to the machine. All other machine types contribute 'whole passes' (such as drum soil compactors).
    /// </summary>
    [JsonIgnore]
    public int FilteredHalfPassCount;

    /// <summary>
    /// Total whole pass count for the cell
    /// </summary>
    [JsonIgnore]
    public int FilteredPassCount;

    /// <summary>
    /// Machine Drive Power value for the cell
    /// </summary>
    [JsonIgnore]
    public short CellMDP;

    /// <summary>
    /// Elevation of the cell pass that recorded the MDP value for the cell
    /// </summary>
    [JsonIgnore]
    public float CellMDPElev;

    /// <summary>
    /// Target CCV value in force at the time of the last cell pass in the cell
    /// </summary>
    [JsonIgnore]
    public short CellTargetCCV;

    /// <summary>
    /// Target MDP value in force at the time of the last cell pass in the cell
    /// </summary>
    [JsonIgnore]
    public short CellTargetMDP;

    /// <summary>
    /// Thickness of the top most layer in the cell.
    /// </summary>
    [JsonIgnore]
    public float CellTopLayerThickness;

    /// <summary>
    /// Elevation of the design at the location of the center point of the cell.
    /// </summary>
    [JsonIgnore]
    public float DesignElev;

    /// <summary>
    /// The elevation information in the cell includes elevations orginated from cell passes provided via production data source from machines.
    /// </summary>
    [JsonIgnore]
    public bool IncludesProductionData;

    /// <summary>
    /// The length of the portion of the profile line that intersects this cell. In the case of cell profile requests, there is no profile line as such, and this value is null.
    /// </summary>
    [JsonIgnore]
    public double InterceptLength;

    /// <summary>
    /// The On The Ground X ordinate of the cell in the cartesian cell address space relative to the coordinate system origin
    /// </summary>
    [JsonIgnore]
    public int OTGCellX;

    /// <summary>
    /// The On The Ground X ordinate of the cell in the cartesian cell address space relative to the coordinate system origin
    /// </summary>
    [JsonIgnore]
    public int OTGCellY;

    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell. In the case of cell profile requests, there is no profile line as such, and this value is null.
    /// </summary>
    [JsonIgnore]
    public double Station;

    /// <summary>
    /// The number of cell passes recorded in the top most (latest) layer in the cell.
    /// </summary>
    [JsonIgnore]
    public int TopLayerPassCount;

    /// <summary>
    /// The target pass count value range at the time of the last cell pass contributed to the cell.
    /// </summary>
    [JsonIgnore]
    public TargetPassCountRange TopLayerPassCountTargetRange;

    /// <summary>
    /// THe set of layers the comprise this cell. Each layer comprises a unique set of cell passes from the filtered pass data and the information calculated from them.
    /// </summary>
    [JsonProperty(PropertyName = "layers")]
    public ProfileLayer[] Layers;

    /// <summary>
    /// Public constructor
    /// </summary>
    public CellPassesResult()
    { }

    public CellPassesResult(
      short cellCCV,
      float cellCCVElev,
      float cellFirstCompositeElev,
      float cellFirstElev,
      float cellHighestCompositeElev,
      float cellHighestElev,
      float cellLastCompositeElev,
      float cellLastElev,
      float cellLowestCompositeElev,
      float cellLowestElev,
      ushort cellMaterialTemperature,
      float cellMaterialTemperatureElev,
      ushort cellMaterialTemperatureWarnMax,
      ushort cellMaterialTemperatureWarnMin,
      int filteredHalfPassCount,
      int filteredPassCount,
      short cellMDP,
      float cellMDPElev,
      short cellTargetCCV,
      short cellTargetMDP,
      float cellTopLayerThickness,
      float designElev,
      bool includesProductionData,
      double interceptLength,
      int oTGCellX,
      int oTGCellY,
      double station,
      int topLayerPassCount,
      TargetPassCountRange topLayerPassCountTargetRange,
      ProfileLayer[] layers
      )
    {
      CellCCV = cellCCV;
      CellCCVElev = cellCCVElev;
      CellFirstCompositeElev = cellFirstCompositeElev;
      CellFirstElev = cellFirstElev;
      CellHighestCompositeElev = cellHighestCompositeElev;
      CellHighestElev = cellHighestElev;
      CellLastCompositeElev = cellLastCompositeElev;
      CellLastElev = cellLastElev;
      CellLowestCompositeElev = cellLowestCompositeElev;
      CellLowestElev = cellLowestElev;
      CellMaterialTemperature = cellMaterialTemperature;
      CellMaterialTemperatureElev = cellMaterialTemperatureElev;
      CellMaterialTemperatureWarnMax = cellMaterialTemperatureWarnMax;
      CellMaterialTemperatureWarnMin = cellMaterialTemperatureWarnMin;
      FilteredHalfPassCount = filteredHalfPassCount;
      FilteredPassCount = filteredPassCount;
      CellMDP = cellMDP;
      CellMDPElev = cellMDPElev;
      CellTargetCCV = cellTargetCCV;
      CellTargetMDP = cellTargetMDP;
      CellTopLayerThickness = cellTopLayerThickness;
      DesignElev = designElev;
      IncludesProductionData = includesProductionData;
      InterceptLength = interceptLength;
      OTGCellX = oTGCellX;
      OTGCellY = oTGCellY;
      Station = station;
      TopLayerPassCount = topLayerPassCount;
      TopLayerPassCountTargetRange = topLayerPassCountTargetRange;
      Layers = layers;
    }
  }
}
