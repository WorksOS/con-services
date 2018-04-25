using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVOICDecls;
using SVOSiteVisionDecls;
using SVOICProfileCell;
using SVOICGridCell;
using SVOICFiltersDecls;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// This is the POST body: A representation of a project extents request
    /// This is copied from ...\ProductionDataSvc.WebAPI\Models\CellPassesRequest.cs
    /// </summary>
    public class CellPassesRequest
    {
        /// <summary>
        /// Project id
        /// </summary>
        public long? projectId { get; set; }

        /// <summary>
        /// Location of the cell in the form of cartesian cell index address. 
        /// May be null.
        /// </summary>       
        public CellAddress cellAddress { get; set; }

        /// <summary>
        /// Location of the cell in the form of a grid position within it. 
        /// May be null.
        /// </summary>   
        public Point probePositionGrid { get; set; }

        /// <summary>
        /// Location of the cell in the form of a WGS84 position within it. 
        /// May be null.
        /// </summary>       
        public WGSPoint probePositionLL { get; set; }

        /// <summary>
        /// The lift/layer build settings to be used.
        /// May be null.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }

        /// <summary>
        /// The type of data being requested for the processed passes and layers to represent.
        /// Defined types are as follows:
        ///  icdtAll = $00000000;
        ///  icdtCCV = $00000001;
        ///  icdtHeight = $00000002;
        ///  icdtLatency = $00000003;
        ///  icdtPassCount = $00000004;
        ///  icdtFrequency = $00000005;
        ///  icdtAmplitude = $00000006;
        ///  icdtMoisture = $00000007;
        ///  icdtTemperature = $00000008;
        ///  icdtRMV = $00000009;
        ///  icdtCCVPercent = $0000000B;
        ///  icdtGPSMode = $0000000A;
        ///  icdtSimpleVolumeOverlay = $0000000C;
        ///  icdtHeightAndTime = $0000000D;
        ///  icdtCompositeHeights = $0000000E;
        ///  icdtMDP = $0000000F;
        ///  icdtMDPPercent = $00000010;
        ///  icdtCellProfile = $00000011;
        ///  icdtCellPasses = $00000012;
        /// </summary>
        public int gridDataType { get; set; }

        /// <summary>
        /// The ID of the filter to be used. 
        /// May be null.
        /// </summary>
        public long? filterId { get; set; }

        /// <summary>
        /// The lift/layer build settings to be used.
        /// May be null.
        /// </summary>
        public FilterResult filter { get; set; }
    } 
    #endregion

    #region Result
    /// <summary>
    /// The collection of information that describe a cell passes profile, as defined by the collection of cell passes that comprise it.
    /// Values are revelant to the cell as a whole. In the case of cell attributes, event or target information they represent the latest known values
    /// for those items as at the time of the last contributory cell pass in the cell that contained a known-value for the attribute in question.
    /// Composite elevations are elevations that are calculated from a combination of elevation information from production data sourced from TAG files produced
    /// by machine control systems, the elevation information obtained from dated topological surveys (surveyed surfaces).
    /// This is copied from ...\ProductionDataSvc.WebAPI\ResultHandling\CellPassesResult.cs
    /// </summary>  
    public class CellPassesResult : RequestResult, IEquatable<CellPassesResult>
    {
        #region Constructor
        public CellPassesResult()
            : base("success")
        { }
        #endregion

        #region Internal Types

        /// <summary>
        /// Contains the information relating to a cell pass. All measurements are made at the center of the cell
        /// </summary>
        public struct CellPassValue
        {
            #region Members
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
            #endregion

            #region Equality test
            public static bool operator ==(CellPassValue a, CellPassValue b)
            {
                return a.amplitude == b.amplitude &&
                    a.cCV == b.cCV &&
                    a.frequency == b.frequency &&
                    a.height == b.height &&
                    a.machineID == b.machineID &&
                    a.machineSpeed == b.machineSpeed &&
                    a.materialTemperature == b.materialTemperature &&
                    a.mDP == b.mDP &&
                    a.radioLatency == b.radioLatency &&
                    a.rMV == b.rMV &&
                    a.time == b.time &&
                    a.gPSModeStore == b.gPSModeStore;
            }

            public static bool operator !=(CellPassValue a, CellPassValue b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return obj is CellPassValue && this == (CellPassValue)obj;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            #endregion
        }

        /// <summary>
        /// The collection of target values set at the time a cell pass is recorded.
        /// </summary>
        public struct CellTargetsValue
        {
            #region Members
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
            #endregion

            #region Equality test
            public static bool operator ==(CellTargetsValue a, CellTargetsValue b)
            {
                return a.targetCCV == b.targetCCV &&
                    a.targetMDP == b.targetMDP &&
                    a.targetPassCount == b.targetPassCount &&
                    a.targetThickness == b.targetThickness &&
                    a.tempWarningLevelMax == b.tempWarningLevelMax &&
                    a.tempWarningLevelMin == b.tempWarningLevelMin;
            }

            public static bool operator !=(CellTargetsValue a, CellTargetsValue b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return obj is CellTargetsValue && this == (CellTargetsValue)obj;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            #endregion
        }

        /// <summary>
        /// The values of temporal event information as at the time the cell pass was recorded.
        /// </summary>
        public struct CellEventsValue
        {
            #region Members
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
            #endregion

            #region Equality test
            public static bool operator ==(CellEventsValue a, CellEventsValue b)
            {
                return a.eventAutoVibrationState == b.eventAutoVibrationState &&
                    a.eventDesignNameID == b.eventDesignNameID &&
                    a.eventICFlags == b.eventICFlags &&
                    a.eventMachineAutomatics == b.eventMachineAutomatics &&
                    a.eventMachineGear == b.eventMachineGear &&
                    a.eventMachineRMVThreshold == b.eventMachineRMVThreshold &&
                    a.eventOnGroundState == b.eventOnGroundState &&
                    a.eventVibrationState == b.eventVibrationState &&
                    a.gPSAccuracy == b.gPSAccuracy &&
                    a.gPSTolerance == b.gPSTolerance &&
                    a.layerID == b.layerID &&
                    a.mapReset_DesignNameID == b.mapReset_DesignNameID &&
                    a.mapReset_PriorDate == b.mapReset_PriorDate &&
                    a.positioningTech == b.positioningTech &&
                    a.EventInAvoidZoneState == b.EventInAvoidZoneState &&
                    a.EventMinElevMapping == b.EventMinElevMapping;
            }

            public static bool operator !=(CellEventsValue a, CellEventsValue b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return obj is CellEventsValue && this == (CellEventsValue)obj;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            #endregion
        }

        /// <summary>
        /// The contains all the cell pass, event and target value information releveant to the cell pass at the time is was recorded.
        /// </summary>
        public class FilteredPassData
        {
            #region Members
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
            #endregion

            #region Equality test
            public static bool operator ==(FilteredPassData a, FilteredPassData b)
            {
                return a.filteredPass == b.filteredPass &&
                    a.eventsValue == b.eventsValue &&
                    a.targetsValue == b.targetsValue;
            }

            public static bool operator !=(FilteredPassData a, FilteredPassData b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return obj is FilteredPassData && this == (FilteredPassData)obj;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            #endregion
        }

        /// <summary>
        /// The collection of information that describe a material layer, as defined by the collection of cell passes that comprise it.
        /// Values are revelant to the layer as a whole. In the case of cell pass attributes, event or target information they represent the latest known values
        /// for those items as at the time of the last contributory cell pass in the layer that contained a known-value for the attribute in question.
        /// </summary>
        public class ProfileLayer
        {
            #region Members
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
            /// The collection of filtered cell pass dcata that comprise this cell. This includes cell pass attributes, target value information and temporal event information for each cell pass.
            /// </summary>
            public FilteredPassData[] filteredPassData;
            #endregion

            #region Equality test
            public static bool operator ==(ProfileLayer a, ProfileLayer b)
            {
                List<FilteredPassData> aFilteredPassData = new List<FilteredPassData>(a.filteredPassData);
                List<FilteredPassData> bFilteredPassData = new List<FilteredPassData>(b.filteredPassData);

                if (aFilteredPassData.Count != bFilteredPassData.Count)
                    return false;
                for (int i = 0; i < aFilteredPassData.Count; ++i)
                {
                    if (!bFilteredPassData.Exists(p => p == aFilteredPassData[i]))
                        return false;
                }

                return a.amplitude == b.amplitude &&
                    a.cCV == b.cCV &&
                    a.cCV_Elev == b.cCV_Elev &&
                    a.cCV_MachineID == b.cCV_MachineID &&
                    a.cCV_Time == b.cCV_Time &&
                    a.filteredHalfPassCount == b.filteredHalfPassCount &&
                    a.filteredPassCount == b.filteredPassCount &&
                    a.firstPassHeight == b.firstPassHeight &&
                    a.frequency == b.frequency &&
                    a.height == b.height &&
                    a.lastLayerPassTime == b.lastLayerPassTime &&
                    a.lastPassHeight == b.lastPassHeight &&
                    a.machineID == b.machineID &&
                    a.materialTemperature == b.materialTemperature &&
                    a.materialTemperature_Elev == b.materialTemperature_Elev &&
                    a.materialTemperature_MachineID == b.materialTemperature_MachineID &&
                    a.materialTemperature_Time == b.materialTemperature_Time &&
                    a.maximumPassHeight == b.maximumPassHeight &&
                    a.maxThickness == b.maxThickness &&
                    a.mDP == b.mDP &&
                    a.mDP_Elev == b.mDP_Elev &&
                    a.mDP_MachineID == b.mDP_MachineID &&
                    a.mDP_Time == b.mDP_Time &&
                    a.minimumPassHeight == b.minimumPassHeight &&
                    a.radioLatency == b.radioLatency &&
                    a.rMV == b.rMV &&
                    a.targetCCV == b.targetCCV &&
                    a.targetMDP == b.targetMDP &&
                    a.targetPassCount == b.targetPassCount &&
                    a.targetThickness == b.targetThickness &&
                    a.thickness == b.thickness;
            }

            public static bool operator !=(ProfileLayer a, ProfileLayer b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return obj is ProfileLayer && this == (ProfileLayer)obj;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            #endregion
        }

        #endregion

        #region Members
        /// <summary>
        /// THe set of layers the comprise this cell. Each layer comprises a unique set of cell passes from the filtered pass data and the information calculated from them.
        /// </summary>
        public ProfileLayer[] layers;
        #endregion

        #region Equality test
        public bool Equals(CellPassesResult other)
        {
            if (other == null)
                return false;

            List<ProfileLayer> thisLayers = new List<ProfileLayer>(this.layers);
            List<ProfileLayer> otherLayer = new List<ProfileLayer>(other.layers);

            if (thisLayers.Count != otherLayer.Count)
                return false;

            for (int i = 0; i < thisLayers.Count; ++i)
            {
                if (!otherLayer.Exists(l => l == thisLayers[i]))
                    return false;
            }

            return this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(CellPassesResult a, CellPassesResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CellPassesResult a, CellPassesResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CellPassesResult && this == (CellPassesResult)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region ToString override
        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        #endregion
    }
    #endregion
}
