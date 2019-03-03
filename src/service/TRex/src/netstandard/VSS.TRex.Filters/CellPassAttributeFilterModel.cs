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
    protected bool _prepared;

    /// <summary>
    /// RequestedGridDataType stores the type of grid data being requested at
    /// the time this filter is asked filter cell passes.
    /// </summary>
    public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

    private bool _HasTimeFilter;
    public bool HasTimeFilter {
      get => _HasTimeFilter;
      set
      {
        _HasTimeFilter = value;
        _prepared = false;
      }
    }

    private bool _HasMachineFilter;
    public bool HasMachineFilter
    {
      get => _HasMachineFilter;
      set
      {
        _HasMachineFilter = value;
        _prepared = false;
      }
    }

    private bool _HasMachineDirectionFilter;
    public bool HasMachineDirectionFilter
    {
      get => _HasMachineDirectionFilter;
      set
      {
        _HasMachineDirectionFilter = value;
        _prepared = false;
      }
    }

    private bool _HasDesignFilter;
    public bool HasDesignFilter
    {
      get => _HasDesignFilter;
      set
      {
        _HasDesignFilter = value;
        _prepared = false;
      }
    }

    private bool _HasVibeStateFilter;
    public bool HasVibeStateFilter
    {
      get => _HasVibeStateFilter;
      set
      {
        _HasVibeStateFilter = value;
        _prepared = false;
      }
    }

    private bool _HasLayerStateFilter;
    public bool HasLayerStateFilter
    {
      get => _HasLayerStateFilter;
      set
      {
        _HasLayerStateFilter = value;
        _prepared = false;
      }
    }

    private bool _HasElevationMappingModeFilter;
    public bool HasElevationMappingModeFilter
    {
      get => _HasElevationMappingModeFilter;
      set
      {
        _HasElevationMappingModeFilter = value;
        _prepared = false;
      }
    }

    private bool _HasElevationTypeFilter;
    public bool HasElevationTypeFilter
    {
      get => _HasElevationTypeFilter;
      set
      {
        _HasElevationTypeFilter = value;
        _prepared = false;
      }
    }

    private bool _HasGCSGuidanceModeFilter;

    public bool HasGCSGuidanceModeFilter
    {
      get => _HasGCSGuidanceModeFilter;
      set
      {
        _HasGCSGuidanceModeFilter = value;
        _prepared = false;
      }
    }

    private bool _HasGPSAccuracyFilter;
    public bool HasGPSAccuracyFilter
    {
      get => _HasGPSAccuracyFilter;
      set
      {
        _HasGPSAccuracyFilter = value;
        _prepared = false;
      }
    }

    private bool _HasGPSToleranceFilter;
    public bool HasGPSToleranceFilter
    {
      get => _HasGPSToleranceFilter;
      set
      {
        _HasGPSToleranceFilter = value;
        _prepared = false;
      }
    }

    private bool _HasPositioningTechFilter;
    public bool HasPositioningTechFilter
    {
      get => _HasPositioningTechFilter;
      set
      {
        _HasPositioningTechFilter = value;
        _prepared = false;
      }
    }

    private bool _HasLayerIDFilter;
    public bool HasLayerIDFilter
    {
      get => _HasLayerIDFilter;
      set
      {
        _HasLayerIDFilter = value;
        _prepared = false;
      }
    }

    private bool _HasElevationRangeFilter;
    public bool HasElevationRangeFilter
    {
      get => _HasElevationRangeFilter;
      set
      {
        _HasElevationRangeFilter = value;
        _prepared = false;
      }
    }

    private bool _HasPassTypeFilter;
    public bool HasPassTypeFilter
    {
      get => _HasPassTypeFilter;
      set
      {
        _HasPassTypeFilter = value;
        _prepared = false;
      }
    }

    private bool _HasCompactionMachinesOnlyFilter;
    public bool HasCompactionMachinesOnlyFilter
    {
      get => _HasCompactionMachinesOnlyFilter;
      set
      {
        _HasCompactionMachinesOnlyFilter = value;
        _prepared = false;
      }
    }

    private bool _HasTemperatureRangeFilter;
    public bool HasTemperatureRangeFilter
    {
      get => _HasTemperatureRangeFilter;
      set
      {
        _HasTemperatureRangeFilter = value;
        _prepared = false;
      }
    }

    private bool _HasPassCountRangeFilter;
    public bool HasPassCountRangeFilter
    {
      get => _HasPassCountRangeFilter;
      set
      {
        _HasPassCountRangeFilter = value;
        _prepared = false;
      }
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
    public Guid[] MachinesList { get; set; } = new Guid[0];

    // Design based filtering member (for designs reported by name from machine via TAG files)
    public int DesignNameID { get; set; } // DesignNameID :TICDesignNameID;

    // Auto Vibe state filtering member
    public VibrationState VibeState { get; set; } = VibrationState.Invalid;

    public MachineDirection MachineDirection { get; set; } = MachineDirection.Unknown;

    public PassTypeSet PassTypeSet { get; set; }

    public ElevationMappingMode ElevationMappingMode { get; set; }
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
    public Guid[] SurveyedSurfaceExclusionList { get; set; } = new Guid[0]; // note this is not saved in the database and must be set in the server

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
    public ushort PassCountRangeMin { get; set; }

    /// <summary>
    ///  takes final filtered passes and reduces to the set to passes within the min max pass count range
    /// </summary>
    public ushort PassCountRangeMax { get; set; }

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

      int machineCount = MachinesList?.Length ?? 0;
      writer.WriteInt(machineCount);
      for (int i = 0; i < machineCount; i++)
        writer.WriteGuid(MachinesList[i]);

      writer.WriteInt(DesignNameID);
      writer.WriteByte((byte)VibeState);
      writer.WriteByte((byte)MachineDirection);
      writer.WriteByte((byte)PassTypeSet);

      writer.WriteByte((byte)ElevationMappingMode);
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

      writer.WriteByte((byte)LayerState);
      writer.WriteInt(LayerID);

      int SurveyedSurfaceExclusionCount = SurveyedSurfaceExclusionList?.Length ?? 0;
      writer.WriteInt(SurveyedSurfaceExclusionCount);
      for (int i = 0; i < SurveyedSurfaceExclusionCount; i++)
        writer.WriteGuid(SurveyedSurfaceExclusionList[i]);

      writer.WriteInt(MaterialTemperatureMin); // No Writer.WriteUShort, use int instead
      writer.WriteInt(MaterialTemperatureMax); // No Writer.WriteUShort, use int instead
      writer.WriteInt(PassCountRangeMin); // No Writer.WriteUShort, use int instead   
      writer.WriteInt(PassCountRangeMax); // No Writer.WriteUShort, use int instead
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

      int machineCount = reader.ReadInt();
      MachinesList = new Guid[machineCount];
      for (int i = 0; i < machineCount; i++)
        MachinesList[i] = reader.ReadGuid() ?? Guid.Empty;

      DesignNameID = reader.ReadInt();
      VibeState = (VibrationState)reader.ReadByte();
      MachineDirection = (MachineDirection)reader.ReadByte();
      PassTypeSet = (PassTypeSet)reader.ReadByte();

      ElevationMappingMode = (ElevationMappingMode)reader.ReadByte();
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

      LayerState = (LayerState)reader.ReadByte();
      LayerID = reader.ReadInt();

      int surveyedSurfaceCount = reader.ReadInt();
      SurveyedSurfaceExclusionList = new Guid[surveyedSurfaceCount];
      for (int i = 0; i < SurveyedSurfaceExclusionList.Length; i++)
        SurveyedSurfaceExclusionList[i] = reader.ReadGuid() ?? Guid.Empty;

      MaterialTemperatureMin = (ushort)reader.ReadInt();
      MaterialTemperatureMax = (ushort)reader.ReadInt();
      PassCountRangeMin = (ushort)reader.ReadInt();
      PassCountRangeMax = (ushort)reader.ReadInt();
    }
  }
}
