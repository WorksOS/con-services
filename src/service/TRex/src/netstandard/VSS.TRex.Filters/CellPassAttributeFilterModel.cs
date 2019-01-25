using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Contains all of the model state relevant to describing the parameters of a cell attribute filter
  /// </summary>
  public class CellPassAttributeFilterModel : ICellPassAttributeFilterModel
  {
    protected bool _prepared = false;

    /// <summary>
    /// RequestedGridDataType stores the type of grid data being requested at
    /// the time this filter is asked filter cell passes.
    /// </summary>
    public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

    public bool HasTimeFilter { get; protected set; }

    public void SetHasTimeFilter(bool state)
    {
      HasTimeFilter = state;
      _prepared = false;
    }

    public bool HasMachineFilter { get; protected set; }

    public void SetHasMachineFilter(bool state)
    {
      HasMachineFilter = state;
      _prepared = false;
    }

    public bool HasMachineDirectionFilter { get; protected set; }

    public void SetHasMachineDirectionFilter(bool state)
    {
      HasMachineDirectionFilter = state;
      _prepared = false;
    }

    public bool HasDesignFilter { get; protected set; }

    public void SetHasDesignFilter(bool state)
    {
      HasDesignFilter = state;
      _prepared = false;
    }

    public bool HasVibeStateFilter { get; protected set; }
    public void SetHasVibeStateFilter(bool state)
    {
      HasVibeStateFilter = state;
      _prepared = false;
    }

    public bool HasLayerStateFilter { get; protected set; }
    public void SetHasLayerStateFilter(bool state)
    {
      HasLayerStateFilter = state;
      _prepared = false;
    }

    public bool HasElevationMappingModeFilter { get; protected set; }
    public void SetHasElevationMappingModeFilter(bool state)
    {
      HasElevationMappingModeFilter = state;
      _prepared = false;
    }

    public bool HasElevationTypeFilter { get; protected set; }
    public void SetHasElevationTypeFilter(bool state)
    {
      HasElevationTypeFilter = state;
      _prepared = false;
    }

    public bool HasGCSGuidanceModeFilter { get; protected set; }
    public void SetHasGCSGuidanceModeFilter(bool state)
    {
      HasGCSGuidanceModeFilter = state;
      _prepared = false;
    }

    public bool HasGPSAccuracyFilter { get; protected set; }
    public void SetHasGPSAccuracyFilter(bool state)
    {
      HasGPSAccuracyFilter = state;
      _prepared = false;
    }

    public bool HasGPSToleranceFilter { get; protected set; }
    public void SetHasGPSToleranceFilter(bool state)
    {
      HasGPSToleranceFilter = state;
      _prepared = false;
    }

    public bool HasPositioningTechFilter { get; protected set; }
    public void SetHasPositioningTechFilter(bool state)
    {
      HasPositioningTechFilter = state;
      _prepared = false;
    }

    public bool HasLayerIDFilter { get; protected set; }
    public void SetHasLayerIDFilter(bool state)
    {
      HasLayerIDFilter = state;
      _prepared = false;
    }

    public bool HasElevationRangeFilter { get; protected set; }
    public void SetHasElevationRangeFilter(bool state)
    {
      HasElevationRangeFilter = state;
      _prepared = false;
    }

    public bool HasPassTypeFilter { get; protected set; }
    public void SetHasPassTypeFilter(bool state)
    {
      HasPassTypeFilter = state;
      _prepared = false;
    }

    public bool HasCompactionMachinesOnlyFilter { get; protected set; }
    public void SetHasCompactionMachinesOnlyFilter(bool state)
    {
      HasCompactionMachinesOnlyFilter = state;
      _prepared = false;
    }

    public bool HasTemperatureRangeFilter { get; protected set; }
    public void SetHasTemperatureRangeFilter(bool state)
    {
      HasTemperatureRangeFilter = state;
      _prepared = false;
    }

    public bool HasPassCountRangeFilter { get; protected set; }
    public void SetHasPassCountRangeFilter(bool state)
    {
      HasPassCountRangeFilter = state;
      _prepared = false;
    }



    public bool FilterTemperatureByLastPass { get; set; }

    public virtual bool IsTimeRangeFilter() => false;

    // Time based filtering members
    /// <summary>
    /// The earliest time that a measured cell pass must have to be included in the filter
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.MinValue;

    /// <summary>
    /// The latest time that a measured cell pass must have to be included in the filter
    /// </summary>
    public DateTime EndTime { get; set; } = DateTime.MaxValue;

    // Machine based filtering members
    public Guid[] MachinesList { get; set; }

    // Design based filtering member (for designs reported by name from machine via TAG files)
    public int DesignNameID { get; set; } // DesignNameID :TICDesignNameID;

    // Auto Vibe state filtering member
    public VibrationState VibeState { get; set; } = VibrationState.Invalid;

    public MachineDirection MachineDirection { get; set; } = MachineDirection.Unknown;

    public PassTypeSet PassTypeSet { get; set; }

    public ElevationMappingMode MinElevationMapping { get; set; }
    public PositioningTech PositioningTech { get; set; } = PositioningTech.Unknown;

    public ushort GPSTolerance { get; set; } = CellPassConsts.NullGPSTolerance;

    public bool GPSAccuracyIsInclusive { get; set; }

    public GPSAccuracy GPSAccuracy { get; set; } = GPSAccuracy.Unknown;

    /// <summary>
    /// The filter will select cell passes with a measure GPS tolerance value greater than the limit specified
    /// in GPSTolerance
    /// </summary>
    public bool GPSToleranceIsGreaterThan { get; set; }

    public ElevationType ElevationType { get; set; } = ElevationType.Last;

    /// <summary>
    /// The machine automatics guidance mode to be in used to record cell passes that will meet the filter.
    /// </summary>
    public MachineAutomaticsMode GCSGuidanceMode { get; set; } = MachineAutomaticsMode.Unknown;

    /// <summary>
    /// ReturnEarliestFilteredCellPass details how we choose a cell pass from a set of filtered
    /// cell passes within a cell. If set, then the first cell pass is chosen. If not set, then
    /// the latest cell pass is chosen
    /// </summary>
    public bool ReturnEarliestFilteredCellPass { get; set; }

    /// <summary>
    /// The elevation to uses as a level benchmark plane for an elevation filter
    /// </summary>
    public double ElevationRangeLevel { get; set; } = Consts.NullDouble;

    /// <summary>
    /// The vertical separation to apply from the benchmark elevation defined as a level or surface elevation
    /// </summary>
    public double ElevationRangeOffset { get; set; } = Consts.NullDouble;

    /// <summary>
    /// The thickness of the range from the level/surface benchmark + Offset to level/surface benchmark + Offset + thickness
    /// </summary>
    public double ElevationRangeThickness { get; set; } = Consts.NullDouble;

    /// <summary>
    /// The design to be used as the benchmark for a surface based elevation range filter
    /// </summary>
    public Guid ElevationRangeDesignUID { get; set; } = Guid.Empty;

    /// <summary>
    /// Elevation parameters have been initialized in preparation for elevation range filtering, either
    /// by setting ElevationRangeBottomElevationForCell and ElevationRangeTopElevationForCell or by
    /// setting ElevationRangeDesignElevations top contain relevant benchmark elevations
    /// </summary>
    public bool ElevationRangeIsInitialised { get; set; }

    /// <summary>
    /// The defined elevation range is defined only by a level plan and thickness
    /// </summary>
    public bool ElevationRangeIsLevelAndThicknessOnly { get; set; }

    /// <summary>
    /// The top of the elevation range permitted for an individual cell being filtered against as
    /// elevation range filter.
    /// </summary>
    public double ElevationRangeTopElevationForCell { get; set; } = Consts.NullDouble;

    /// <summary>
    /// The bottom of the elevation range permitted for an individual cell being filtered against as
    /// elevation range filter.
    /// </summary>
    public double ElevationRangeBottomElevationForCell { get; set; } = Consts.NullDouble;

    /// <summary>
    /// Denotes whether analysis of cell passes in a cell are analyzed into separate layers according to 
    /// LayerMethod or if extracted cell passes are wrapped into a single containing layer.
    /// </summary>
    public LayerState LayerState { get; set; } = LayerState.Invalid;

    /// <summary>
    /// ID of layer we are only interested in
    /// </summary>
    public int LayerID { get; set; } = -1;

    /// <summary>
    /// The list of surveyed surface identifiers to be excluded from the filtered result
    /// </summary>
    public Guid[] SurveyedSurfaceExclusionList { get; set; } // note this is not saved in the database and must be set in the server

    /// <summary>
    /// Only permit cell passes for temperature values within min max range
    /// </summary>
    public ushort MaterialTemperatureMin { get; set; }

    /// <summary>
    /// Only permit cell passes for temperature values within min max range
    /// </summary>
    public ushort MaterialTemperatureMax { get; set; }

    /// <summary>
    /// takes final filtered passes and reduces to the set to passes within the min max pass count range
    /// </summary>
    public ushort PasscountRangeMin { get; set; }

    /// <summary>
    ///  takes final filtered passes and reduces to the set to passes within the min max pass count range
    /// </summary>
    public ushort PasscountRangeMax { get; set; }

    /// <summary>
    /// Serialize the state of the cell pass attribute filter using the FromToBinary serialization approach
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      const byte VERSION_NUMBER = 1;
      writer.WriteByte(VERSION_NUMBER);

      writer.WriteByte((byte) RequestedGridDataType);
      writer.WriteBoolean(HasTimeFilter);
      writer.WriteBoolean(HasMachineFilter);
      writer.WriteBoolean(HasMachineDirectionFilter);
      writer.WriteBoolean(HasDesignFilter);
      writer.WriteBoolean(HasVibeStateFilter);
      writer.WriteBoolean(HasLayerStateFilter);
      writer.WriteBoolean(HasElevationMappingModeFilter);
      writer.WriteBoolean(HasElevationTypeFilter);
      writer.WriteBoolean(HasGCSGuidanceModeFilter);
      writer.WriteBoolean(HasGPSAccuracyFilter);
      writer.WriteBoolean(HasGPSToleranceFilter);
      writer.WriteBoolean(HasPositioningTechFilter);
      writer.WriteBoolean(HasLayerIDFilter);
      writer.WriteBoolean(HasElevationRangeFilter);
      writer.WriteBoolean(HasPassTypeFilter);
      writer.WriteBoolean(HasCompactionMachinesOnlyFilter);
      writer.WriteBoolean(HasTemperatureRangeFilter);
      writer.WriteBoolean(FilterTemperatureByLastPass);
      writer.WriteBoolean(HasPassCountRangeFilter);

      writer.WriteLong(StartTime.ToBinary());
      writer.WriteLong(EndTime.ToBinary());

      writer.WriteBoolean(MachinesList != null);
      if (MachinesList != null)
      {
        writer.WriteInt(MachinesList.Length);
        for (int i = 0; i < MachinesList.Length; i++)
          writer.WriteGuid(MachinesList[i]);
      }

      writer.WriteInt(DesignNameID);
      writer.WriteByte((byte)VibeState);
      writer.WriteByte((byte)MachineDirection);
      writer.WriteByte((byte)PassTypeSet);

      writer.WriteByte((byte)MinElevationMapping);
      writer.WriteByte((byte)PositioningTech);
      writer.WriteInt(GPSTolerance); // No WriteUShort is provided, use an int...
      writer.WriteBoolean(GPSAccuracyIsInclusive);
      writer.WriteByte((byte)GPSAccuracy);
      writer.WriteBoolean(GPSToleranceIsGreaterThan);

      writer.WriteByte((byte)ElevationType);
      writer.WriteByte((byte)GCSGuidanceMode);

      writer.WriteBoolean(ReturnEarliestFilteredCellPass);

      writer.WriteDouble(ElevationRangeLevel);
      writer.WriteDouble(ElevationRangeOffset);
      writer.WriteDouble(ElevationRangeThickness);

      writer.WriteGuid(ElevationRangeDesignUID);

      writer.WriteBoolean(ElevationRangeIsInitialised);
      writer.WriteBoolean(ElevationRangeIsLevelAndThicknessOnly);

      writer.WriteDouble(ElevationRangeTopElevationForCell);
      writer.WriteDouble(ElevationRangeBottomElevationForCell);

      writer.WriteByte((byte)LayerState);
      writer.WriteInt(LayerID);

      writer.WriteBoolean(SurveyedSurfaceExclusionList != null);
      if (SurveyedSurfaceExclusionList != null)
      {
        writer.WriteInt(SurveyedSurfaceExclusionList.Length);
        for (int i = 0; i < (SurveyedSurfaceExclusionList.Length); i++)
          writer.WriteGuid(SurveyedSurfaceExclusionList[i]);
      }

      writer.WriteInt(MaterialTemperatureMin); // No Writer.WriteUShort, use int instead
      writer.WriteInt(MaterialTemperatureMax); // No Writer.WriteUShort, use int instead
      writer.WriteInt(PasscountRangeMin); // No Writer.WriteUShort, use int instead   
      writer.WriteInt(PasscountRangeMax); // No Writer.WriteUShort, use int instead
    }

    /// <summary>
    /// Deserialize the state of the cell pass attribute filter using the FromToBinary serialization approach
    /// </summary>
    public void FromBinary(IBinaryRawReader reader)
    {
      const byte VERSION_NUMBER = 1;

      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, readVersionNumber);

      RequestedGridDataType = (GridDataType)reader.ReadByte();
      HasTimeFilter = reader.ReadBoolean();
      HasMachineFilter = reader.ReadBoolean();
      HasMachineDirectionFilter = reader.ReadBoolean();
      HasDesignFilter = reader.ReadBoolean();
      HasVibeStateFilter = reader.ReadBoolean();
      HasLayerStateFilter = reader.ReadBoolean();
      HasElevationMappingModeFilter = reader.ReadBoolean();
      HasElevationTypeFilter = reader.ReadBoolean();
      HasGCSGuidanceModeFilter = reader.ReadBoolean();
      HasGPSAccuracyFilter = reader.ReadBoolean();
      HasGPSToleranceFilter = reader.ReadBoolean();
      HasPositioningTechFilter = reader.ReadBoolean();
      HasLayerIDFilter = reader.ReadBoolean();
      HasElevationRangeFilter = reader.ReadBoolean();
      HasPassTypeFilter = reader.ReadBoolean();
      HasCompactionMachinesOnlyFilter = reader.ReadBoolean();
      HasTemperatureRangeFilter = reader.ReadBoolean();
      FilterTemperatureByLastPass = reader.ReadBoolean();
      HasPassCountRangeFilter = reader.ReadBoolean();

      StartTime = DateTime.FromBinary(reader.ReadLong());
      EndTime = DateTime.FromBinary(reader.ReadLong());

      if (reader.ReadBoolean())
      {
        int machineCount = reader.ReadInt();
        MachinesList = new Guid[machineCount];
        for (int i = 0; i < machineCount; i++)
          MachinesList[i] = reader.ReadGuid() ?? Guid.Empty;
      }

      DesignNameID = reader.ReadInt();
      VibeState = (VibrationState)reader.ReadByte();
      MachineDirection = (MachineDirection)reader.ReadByte();
      PassTypeSet = (PassTypeSet)reader.ReadByte();

      MinElevationMapping = (ElevationMappingMode)reader.ReadByte();
      PositioningTech = (PositioningTech)reader.ReadByte();
      GPSTolerance = (ushort)reader.ReadInt();
      GPSAccuracyIsInclusive = reader.ReadBoolean();
      GPSAccuracy = (GPSAccuracy)reader.ReadByte();
      GPSToleranceIsGreaterThan = reader.ReadBoolean();

      ElevationType = (ElevationType)reader.ReadByte();
      GCSGuidanceMode = (MachineAutomaticsMode)reader.ReadByte();

      ReturnEarliestFilteredCellPass = reader.ReadBoolean();

      ElevationRangeLevel = reader.ReadDouble();
      ElevationRangeOffset = reader.ReadDouble();
      ElevationRangeThickness = reader.ReadDouble();

      ElevationRangeDesignUID = reader.ReadGuid() ?? Guid.Empty;

      ElevationRangeIsInitialised = reader.ReadBoolean();
      ElevationRangeIsLevelAndThicknessOnly = reader.ReadBoolean();

      ElevationRangeTopElevationForCell = reader.ReadDouble();
      ElevationRangeBottomElevationForCell = reader.ReadDouble();

      LayerState = (LayerState)reader.ReadByte();
      LayerID = reader.ReadInt();

      if (reader.ReadBoolean())
      {
        int surveyedSurfaceCount = reader.ReadInt();
        SurveyedSurfaceExclusionList = new Guid[surveyedSurfaceCount];
        for (int i = 0; i < SurveyedSurfaceExclusionList.Length; i++)
          SurveyedSurfaceExclusionList[i] = reader.ReadGuid() ?? Guid.Empty;
      }

      MaterialTemperatureMin = (ushort)reader.ReadInt();
      MaterialTemperatureMax = (ushort)reader.ReadInt();
      PasscountRangeMin = (ushort)reader.ReadInt();
      PasscountRangeMax = (ushort)reader.ReadInt();
    }
  }
}
