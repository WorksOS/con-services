using Newtonsoft.Json;
using SVOICDecls;
using SVOSiteVisionDecls;
using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

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
      public ushort amplitude;

      /// <summary>
      /// The CCV measured by the machine. Value is expressed in 10ths of units
      /// </summary>
      public short cCV;

      /// <summary>
      /// The measured frequency of the compaction drum vibration. The value is expressed in 100ths of milliters
      /// </summary>
      public ushort frequency;

      /// <summary>
      /// The elevation of the cell pass with respect to datum of the grid coordinate sysem. The value is expressed in Meters.
      /// </summary>
      public float height;

      /// <summary>
      /// The numeric identifier assigned to the machine at the time the TAG file containing the cellk pass is processed
      /// </summary>
      public long machineID;

      /// <summary>
      /// The calculated speed of the machine at the time the cell pass is measured, measured in centimeters per second.
      /// </summary>
      public ushort machineSpeed;

      /// <summary>
      /// The temperature of the asphalt mat at the time the asphalt compactor rolled over it.
      /// </summary>
      public ushort materialTemperature;

      /// <summary>
      /// The MDP measured by the machine. Value is expressed in 10ths of units
      /// </summary>
      public short mDP;

      /// <summary>
      /// The radio latency measured at the time the cell was passed over by the machine. Expressed in seconds since the RTK correction was emitted from the base station.
      /// </summary>
      public byte radioLatency;

      /// <summary>
      /// The Resonanace Meter Value measured by a compaction machine when the vibratory rolled over the cell.
      /// </summary>
      public short rMV;

      /// <summary>
      /// The time at which the cell was rolled over in this pass. For GPS equipped systems this is GPS time. For ATS/UTS equiped systems this is the GCS internal system clock time. 
      /// The time is expressed un UTC.
      /// </summary>
      public DateTime time;

      /// <summary>
      /// A bit field stored used to hold the GPSMode recorded by the machine at the time the cell was passed over. The LSB four bits in the byte are used for this purpose.
      /// </summary>
      public byte gPSModeStore;
    }

    /// <summary>
    /// The collection of target values set at the time a cell pass is recorded.
    /// </summary>
    public class CellTargetsValue
    {
      /// <summary>
      /// The target CCV. In 10ths of units.
      /// </summary>
      public short targetCCV;

      /// <summary>
      /// The target MDP. In 10ths of units.
      /// </summary>
      public short targetMDP;

      /// <summary>
      /// The target pass count to attain before material is considered to be compacted sufficiently.
      /// </summary>
      public ushort targetPassCount;

      /// <summary>
      /// Target lift thickness for each layer. Value is expressed in meters.
      /// </summary>
      public float targetThickness;

      /// <summary>
      /// The upper bound of the asphalt temperature mat when being rolled. Values above this cause a warning to be issued on the machine. Expressed in degrees Celcius.
      /// </summary>
      public ushort tempWarningLevelMax;

      /// <summary>
      /// The lower bound of the asphalt temperature mat when being rolled. Values below this cause a warning to be issued on the machine. Expressed in degrees Celcius.
      /// </summary>
      public ushort tempWarningLevelMin;
    }

    /// <summary>
    /// The values of temporal event information as at the time the cell pass was recorded.
    /// </summary>
    public class CellEventsValue
    {
      /// <summary>
      /// Is the compactor using automatic vibration control
      /// </summary>
      public TICAutoState eventAutoVibrationState;

      /// <summary>
      /// The ID of the design loaded machine. This is a foreign key into the design collection maintained in the project.
      /// </summary>
      public int eventDesignNameID;

      /// <summary>
      /// The intelligent compaction flags set emitted by the machine. Structure and meaning is dependent on the compaction sensor type installed on the compaction machine.
      /// </summary>
      public byte eventICFlags;

      /// <summary>
      /// The GCS automatics control mode - manual (indicate only) or automatics (blade control)
      /// </summary>
      public TGCSAutomaticsMode eventMachineAutomatics;

      /// <summary>
      /// The gear the machine is in
      /// </summary>
      public TICMachineGear eventMachineGear;

      /// <summary>
      /// The RMV threshold reported by the machine. Values above this level are interpreted as decoupled (the compactor drum is bouncing)
      /// </summary>
      public short eventMachineRMVThreshold;

      /// <summary>
      /// Is the machine implemnent (drum, blade, bucket etc) in contact with the ground when the measurement is made. If not decoupled, a compactor drum is considered to be on the groudn by definition.
      /// </summary>
      public TICOnGroundState eventOnGroundState;

      /// <summary>
      /// Is the compactor drum in a vibratory state?
      /// </summary>
      public TICVibrationState eventVibrationState;

      /// <summary>
      /// The GSP accuracy mode the GCS system is operating under. Fine, medium and coarse relate to accuracy ranges which may be millimeters at the Fine end, and meters are the coarse end.
      /// </summary>
      public TICGPSAccuracy gPSAccuracy;

      /// <summary>
      /// The GPS tolerance, or error limit, of GPS positions being used. This is a value in the range 0..2^14-1 millimeters.
      /// </summary>
      public ushort gPSTolerance;

      /// <summary>
      /// The layer number entered by the operator on the machine.
      /// </summary>
      public ushort layerID;

      /// <summary>
      /// The ID of the design loaded at the time the user executed a map reset on the machine. This is a foreign key into the design collection maintained in the project.
      /// </summary>
      public int mapReset_DesignNameID;

      /// <summary>
      /// The date of the most recent map reset event prior to the time the cell pass is recorded.
      /// </summary>
      public DateTime mapReset_PriorDate;

      /// <summary>
      /// The positioning technology used to collect the implement (drum, blade etc) position. The current values are GPS or UTS (Universal Total Station)
      /// </summary>
      public TICPositioningTech positioningTech;

      /// <summary>
      /// A set of bit flags that indicate avoidance zone transgression states. Not currently implemented. Documented elsewhere.
      /// </summary>
      public byte EventInAvoidZoneState;

      /// <summary>
      /// The GCS machine control system is mapping minimum elevations; typically used by HEX (Hydraulic Excavator machines)
      /// </summary>
      public bool EventMinElevMapping;
    }

    /// <summary>
    /// The contains all the cell pass, event and target value information releveant to the cell pass at the time is was recorded.
    /// </summary>
    public class FilteredPassData
    {
      /// <summary>
      /// Attributes values for the cell pass
      /// </summary>
      public CellPassValue filteredPass;

      /// <summary>
      /// Values of temporal event information for the cell pass
      /// </summary>
      public CellEventsValue eventsValue;

      /// <summary>
      /// Values of attribute target values for the cell pass
      /// </summary>
      public CellTargetsValue targetsValue;
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
          public ushort amplitude;

          /// <summary>
          /// CCV value for the layer. Expressed in 10ths of units.
          /// </summary>
          public short cCV;

          /// <summary>
          /// Elevation of the cell pass that contributed the layer CCV value
          /// </summary>
          public float cCV_Elev;

          /// <summary>
          /// The ID of the machine that recorded the cell pass that contributed the CCV value
          /// </summary>
          public long cCV_MachineID;

          /// <summary>
          /// The time the cell pass was recorded that contributed the CCV value.
          /// </summary>
          public DateTime cCV_Time;

          /// <summary>
          /// The number of 'half passes' counted in the layer. A pass made by certain machine types (eg: CTCT GCS Quattro four wheel landfill sheepsfoot compactor) are counted as half passes 
          /// (each axle contributes half of the compactive effort attributed to the machine. All other machine types contribute 'whole passes' (such as drum soil compactors).
          /// </summary>
          public int filteredHalfPassCount;

          /// <summary>
          /// The rounded whole pass count for the layer. Residual half pass counts are rouned up (eg: 3 half passes is rounded up to 2 whole passes)
          /// </summary>
          public int filteredPassCount;

          /// <summary>
          /// The elevation of the first cell pass recorded in the layer.
          /// </summary>
          public float firstPassHeight;

          /// <summary>
          /// The vibratory drum frequency. Value is expressed in 10ths of Hertz.
          /// </summary>
          public ushort frequency;

          /// <summary>
          /// The measured elevation of the last cell pass made in the layer. This represents the best known elevation of the top of the material layer at the location of the cell.
          /// </summary>
          public float height;

          /// <summary>
          /// The time at which the last cell pass ccntributed to this layer was recorded.
          /// </summary>
          public DateTime lastLayerPassTime;

          /// <summary>
          /// The elevation of the last cell pass contributed to the layer.
          /// </summary>
          public float lastPassHeight;

          /// <summary>
          /// The machine ID of the last cell pass contributed to the layer
          /// </summary>
          public long machineID;

          /// <summary>
          /// The material temperature (recorded by asphalt compactors).
          /// </summary>
          public ushort materialTemperature;

          /// <summary>
          /// Elevation of the cell pass that recorded the material temperature for the layer
          /// </summary>
          public float materialTemperature_Elev;

          /// <summary>
          /// The ID of the machine that recorded the material temperature for the layer
          /// </summary>
          public long materialTemperature_MachineID;

          /// <summary>
          /// The time the cell pass that contributed the material temperature value
          /// </summary>
          public DateTime materialTemperature_Time;

          /// <summary>
          /// The maximum elevation recorded across all cell passes contributed to the layer.
          /// </summary>
          public float maximumPassHeight;

          /// <summary>
          /// The maximum layer thickness recorded across all cell passes contributed to the layer.
          /// </summary>
          public float maxThickness;

          /// <summary>
          /// MDP value for the layer. Epressed in 10ths of units.
          /// </summary>
          public short mDP;

          /// <summary>
          /// Elevation of the cell pass that contributed the layer MDP value
          /// </summary>
          public float mDP_Elev;

          /// <summary>
          /// The ID of the machine that recorded the cell pass that contributed the MDP value
          /// </summary>
          public long mDP_MachineID;

          /// <summary>
          /// The time the cell pass was recorded that contributed the CCV value.
          /// </summary>
          public DateTime mDP_Time;

          /// <summary>
          /// The lowest elevation value recorded across all cell passes contributed to the layer.
          /// </summary>
          public float minimumPassHeight;

          /// <summary>
          /// The radio latency for the layer
          /// </summary>
          public byte radioLatency;

          /// <summary>
          /// The resonance meter value for the layer
          /// </summary>
          public short rMV;

          /// <summary>
          /// The target CCV value used for the layer
          /// </summary>
          public short targetCCV;

          /// <summary>
          /// The target MDP value used for the layer
          /// </summary>
          public short targetMDP;

          /// <summary>
          /// The target pass count value used for the layer
          /// </summary>
          public int targetPassCount;

          /// <summary>
          /// The target layer thickness value used for the layer
          /// </summary>
          public float targetThickness;

          /// <summary>
          /// The final recorded thickness for the layer once all cell passes have been contributed to it
          /// </summary>
          public float thickness;

          /// <summary>
          /// The collection of filtered cell pass data that comprise this cell. This includes cell pass attributes, target value information and temporal event information for each cell pass.
          /// </summary>
          public FilteredPassData[] filteredPassData;
      }

      /// <summary>
      /// The internal result code returned by the service for the request. Values documented elsewhere.
      /// </summary>
      [JsonIgnore]
      public int returnCode;

      /// <summary>
      /// CCV value
      /// </summary>
      [JsonIgnore]
      public short cellCCV;

      /// <summary>
      /// Elevation from cell pass that contributed the CCV
      /// </summary>
      [JsonIgnore]
      public float cellCCVElev;

      /// <summary>
      /// First (in time) composite elevation recorded in the cell
      /// </summary>
      [JsonIgnore]
      public float cellFirstCompositeElev;

      /// <summary>
      /// First (in time) production data only elevation recorded in the cell
      /// </summary>
      [JsonIgnore]
      public float cellFirstElev;

      /// <summary>
      /// Highest composite elevation recorded in the cell
      /// </summary>
      [JsonIgnore]
      public float cellHighestCompositeElev;

      /// <summary>
      /// Highest production data elevation recorded in the cell
      /// </summary>
      [JsonIgnore]
      public float cellHighestElev;

      /// <summary>
      /// Last (in time) composite elevation recorded in the cell
      /// </summary>
      [JsonIgnore]
      public float cellLastCompositeElev;

      /// <summary>
      /// Last (in time) production data only elevation recorded in the cell
      /// </summary>
      [JsonIgnore]
      public float cellLastElev;

      /// <summary>
      /// Lowest composite elevation recorded in the cell
      /// </summary>
      [JsonIgnore]
      public float cellLowestCompositeElev;

      /// <summary>
      /// Lowest production data elevation recorded in the cell
      /// </summary>
      [JsonIgnore]
      public float cellLowestElev;

      /// <summary>
      /// Material temperature for the cell
      /// </summary>
      [JsonIgnore]
      public ushort cellMaterialTemperature;

      /// <summary>
      /// Elevation of the cell pass that recorded the material temperature for the cell
      /// </summary>
      [JsonIgnore]
      public float cellMaterialTemperatureElev;

      /// <summary>
      /// Maximum material temperature warning for the cell
      /// </summary>
      [JsonIgnore]
      public ushort cellMaterialTemperatureWarnMax;

      /// <summary>
      /// Minimum material temperature warning for the cell
      /// </summary>
      [JsonIgnore]
      public ushort cellMaterialTemperatureWarnMin;

      /// <summary>
      /// Total filtered half pass count for the cell
      /// A pass made by certain machine types (eg: CTCT GCS Quattro four wheel landfill sheepsfoot compactor) are counted as half passes 
      /// (each axle contributes half of the compactive effort attributed to the machine. All other machine types contribute 'whole passes' (such as drum soil compactors).
      /// </summary>
      [JsonIgnore]
      public int filteredHalfPassCount;

      /// <summary>
      /// Total whole pass count for the cell
      /// </summary>
      [JsonIgnore]
      public int filteredPassCount;

      /// <summary>
      /// Machine Drive Power value for the cell
      /// </summary>
      [JsonIgnore]
      public short cellMDP;

      /// <summary>
      /// Elevation of the cell pass that recorded the MDP value for the cell
      /// </summary>
      [JsonIgnore]
      public float cellMDPElev;

      /// <summary>
      /// Target CCV value in force at the time of the last cell pass in the cell
      /// </summary>
      [JsonIgnore]
      public short cellTargetCCV;

      /// <summary>
      /// Target MDP value in force at the time of the last cell pass in the cell
      /// </summary>
      [JsonIgnore]
      public short cellTargetMDP;

      /// <summary>
      /// Thickness of the top most layer in the cell.
      /// </summary>
      [JsonIgnore]
      public float cellTopLayerThickness;

      /// <summary>
      /// Elevation of the design at the location of the center point of the cell.
      /// </summary>
      [JsonIgnore]
      public float designElev;

      /// <summary>
      /// The elevation information in the cell includes elevations orginated from cell passes provided via production data source from machines.
      /// </summary>
      [JsonIgnore]
      public bool includesProductionData;

      /// <summary>
      /// The length of the portion of the profile line that intersects this cell. In the case of cell profile requests, there is no profile line as such, and this value is null.
      /// </summary>
      [JsonIgnore]
      public double interceptLength;

      /// <summary>
      /// The On The Ground X ordinate of the cell in the cartesian cell address space relative to the coordinate system origin
      /// </summary>
      [JsonIgnore]
      public int oTGCellX;

      /// <summary>
      /// The On The Ground X ordinate of the cell in the cartesian cell address space relative to the coordinate system origin
      /// </summary>
      [JsonIgnore]
      public int oTGCellY;

      /// <summary>
      /// The station value, or distance from start of the profile line at which the profile line intersects this cell. In the case of cell profile requests, there is no profile line as such, and this value is null.
      /// </summary>
      [JsonIgnore]
      public double station;

      /// <summary>
      /// The number of cell passes recorded in the top most (latest) layer in the cell.
      /// </summary>
      [JsonIgnore]
      public int topLayerPassCount;

      /// <summary>
      /// The target pass count value range at the time of the last cell pass contributed to the cell.
      /// </summary>
      [JsonIgnore]
      public TargetPassCountRange topLayerPassCountTargetRange;

 

      /// <summary>
      /// THe set of layers the comprise this cell. Each layer comprises a unique set of cell passes from the filtered pass data and the information calculated from them.
      /// </summary>
      public ProfileLayer[] layers;

           /// <summary>
        /// Private constructor
        /// </summary>
    private CellPassesResult()
        {}

    public static CellPassesResult CreateCellPassesResult(
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
      return new CellPassesResult
      {
        cellCCV = cellCCV,
        cellCCVElev = cellCCVElev,
        cellFirstCompositeElev = cellFirstCompositeElev,
        cellFirstElev = cellFirstElev,
        cellHighestCompositeElev = cellHighestCompositeElev,
        cellHighestElev = cellHighestElev,
        cellLastCompositeElev = cellLastCompositeElev,
        cellLastElev = cellLastElev,
        cellLowestCompositeElev = cellLowestCompositeElev,
        cellLowestElev = cellLowestElev,
        cellMaterialTemperature = cellMaterialTemperature,
        cellMaterialTemperatureElev = cellMaterialTemperatureElev,
        cellMaterialTemperatureWarnMax = cellMaterialTemperatureWarnMax,
        cellMaterialTemperatureWarnMin = cellMaterialTemperatureWarnMin,
        filteredHalfPassCount = filteredHalfPassCount,
        filteredPassCount = filteredPassCount,
        cellMDP = cellMDP,
        cellMDPElev = cellMDPElev,
        cellTargetCCV = cellTargetCCV,
        cellTargetMDP = cellTargetMDP,
        cellTopLayerThickness = cellTopLayerThickness,
        designElev = designElev,
        includesProductionData = includesProductionData,
        interceptLength = interceptLength,
        oTGCellX = oTGCellX,
        oTGCellY = oTGCellY,
        station = station,
        topLayerPassCount = topLayerPassCount,
        topLayerPassCountTargetRange = topLayerPassCountTargetRange,
        layers = layers
      };
    }
  }
}
