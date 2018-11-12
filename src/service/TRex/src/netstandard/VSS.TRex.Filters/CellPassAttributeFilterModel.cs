using System;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Contains all of the model state relevant to describing the parameters of a cell attribute filter
  /// </summary>
  public class CellPassAttributeFilterModel : ICellPassAttributeFilterModel
  {
    /// <summary>
    /// RequestedGridDataType stores the type of grid data being requested at
    /// the time this filter is asked filter cell passes.
    /// </summary>
    public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

    public bool HasTimeFilter { get; set; }
    public bool HasMachineFilter { get; set; }
    public bool HasMachineDirectionFilter { get; set; }
    public bool HasDesignFilter { get; set; }
    public bool HasVibeStateFilter { get; set; }
    public bool HasLayerStateFilter { get; set; }
    public bool HasMinElevMappingFilter { get; set; }
    public bool HasElevationTypeFilter { get; set; }
    public bool HasGCSGuidanceModeFilter { get; set; }
    public bool HasGPSAccuracyFilter { get; set; }
    public bool HasGPSToleranceFilter { get; set; }
    public bool HasPositioningTechFilter { get; set; }
    public bool HasLayerIDFilter { get; set; }
    public bool HasElevationRangeFilter { get; set; }
    public bool HasPassTypeFilter { get; set; }

    public virtual bool IsTimeRangeFilter() => false;

    public bool HasCompactionMachinesOnlyFilter { get; set; }

    public bool HasTemperatureRangeFilter { get; set; }

    public bool FilterTemperatureByLastPass { get; set; }

    public bool HasPassCountRangeFilter { get; set; }

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

    public bool MinElevationMapping { get; set; } //MinElevationMapping : TICMinElevMappingState;
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
    public Guid ElevationRangeDesignID { get; set; } = Guid.Empty;

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
    /// Only permit cell passes recorded from a compaction type machine to be considered for filtering
    /// </summary>
    public bool RestrictFilteredDataToCompactorsOnly { get; set; }

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
    public ushort PasscountRangeMin { get; set; }

    /// <summary>
    ///  takes final filtered passes and reduces to the set to passes within the min max pass count range
    /// </summary>
    public ushort PasscountRangeMax { get; set; }

    /// <summary>
    /// Serialise the state of the cell pass attribute filter using the FromToBinary serialization approach
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;
      writer.WriteByte(versionNumber);

      writer.WriteInt((int) RequestedGridDataType);
      writer.WriteBoolean(HasTimeFilter);
      writer.WriteBoolean(HasMachineFilter);
      writer.WriteBoolean(HasMachineDirectionFilter);
      writer.WriteBoolean(HasDesignFilter);
      writer.WriteBoolean(HasVibeStateFilter);
      writer.WriteBoolean(HasLayerStateFilter);
      writer.WriteBoolean(HasMinElevMappingFilter);
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

      writer.WriteLong(StartTime.Ticks);
      writer.WriteLong(EndTime.Ticks);

      writer.WriteBoolean(MachinesList != null);
      if (MachinesList != null)
      { 
        writer.WriteInt(MachinesList.Length);
        for (var i = 0; i < (MachinesList.Length); i++)
          writer.WriteGuid(MachinesList[i]);
      }
      
      writer.WriteInt(DesignNameID);
      writer.WriteInt((int)VibeState);
      writer.WriteInt((int)MachineDirection);
      writer.WriteInt((int)PassTypeSet);

      writer.WriteBoolean(MinElevationMapping);
      writer.WriteInt((int)PositioningTech);
      writer.WriteInt(GPSTolerance); // No WriteUShort is provided, use an int...
      writer.WriteBoolean(GPSAccuracyIsInclusive);
      writer.WriteInt((int)GPSAccuracy);
      writer.WriteBoolean(GPSToleranceIsGreaterThan);

      writer.WriteInt((int)ElevationType);
      writer.WriteInt((int)GCSGuidanceMode);

      writer.WriteBoolean(ReturnEarliestFilteredCellPass);

      writer.WriteDouble(ElevationRangeLevel);
      writer.WriteDouble(ElevationRangeOffset);
      writer.WriteDouble(ElevationRangeThickness);

      writer.WriteGuid(ElevationRangeDesignID);

      writer.WriteBoolean(ElevationRangeIsInitialised);
      writer.WriteBoolean(ElevationRangeIsLevelAndThicknessOnly);

      writer.WriteDouble(ElevationRangeTopElevationForCell);
      writer.WriteDouble(ElevationRangeBottomElevationForCell);

      writer.WriteInt((int)LayerState);
      writer.WriteInt(LayerID);

      writer.WriteBoolean(RestrictFilteredDataToCompactorsOnly);

      writer.WriteInt(SurveyedSurfaceExclusionList?.Length ?? 0);
      for (int i = 0; i < (SurveyedSurfaceExclusionList?.Length ?? 0); i++)
        writer.WriteGuid(SurveyedSurfaceExclusionList?[i]);

      writer.WriteInt(MaterialTemperatureMin); // No Writer.WriteUShort, use int instead
      writer.WriteInt(MaterialTemperatureMax); // No Writer.WriteUShort, use int instead
      writer.WriteInt(PasscountRangeMin); // No Writer.WriteUShort, use int instead   
      writer.WriteInt(PasscountRangeMax); // No Writer.WriteUShort, use int instead
    }

    /// <summary>
    /// Deserialise the state of the cell pass attribute filter using the FromToBinary serialization approach
    /// </summary>
    public void FromBinary(IBinaryRawReader reader)
    {
      const byte versionNumber = 1;

      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      RequestedGridDataType = (GridDataType)reader.ReadInt();
      HasTimeFilter = reader.ReadBoolean();
      HasMachineFilter = reader.ReadBoolean();
      HasMachineDirectionFilter = reader.ReadBoolean();
      HasDesignFilter = reader.ReadBoolean();
      HasVibeStateFilter = reader.ReadBoolean();
      HasLayerStateFilter = reader.ReadBoolean();
      HasMinElevMappingFilter = reader.ReadBoolean();
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

      StartTime = new DateTime(reader.ReadLong());
      EndTime = new DateTime(reader.ReadLong());

      if (reader.ReadBoolean())
      {
        var machineCount = reader.ReadInt();
        if (machineCount > 0)
        {
          MachinesList = new Guid[machineCount];
          for (var i = 0; i < MachinesList.Length; i++)
            MachinesList[i] = reader.ReadGuid() ?? Guid.Empty;
        }
      }

      DesignNameID = reader.ReadInt();
      VibeState = (VibrationState)reader.ReadInt();
      MachineDirection = (MachineDirection)reader.ReadInt();
      PassTypeSet = (PassTypeSet)reader.ReadInt();

      MinElevationMapping = reader.ReadBoolean();
      PositioningTech = (PositioningTech)reader.ReadInt();
      GPSTolerance = (ushort)reader.ReadInt();
      GPSAccuracyIsInclusive = reader.ReadBoolean();
      GPSAccuracy = (GPSAccuracy)reader.ReadInt();
      GPSToleranceIsGreaterThan = reader.ReadBoolean();

      ElevationType = (ElevationType)reader.ReadInt();
      GCSGuidanceMode = (MachineAutomaticsMode)reader.ReadInt();

      ReturnEarliestFilteredCellPass = reader.ReadBoolean();

      ElevationRangeLevel = reader.ReadDouble();
      ElevationRangeOffset = reader.ReadDouble();
      ElevationRangeThickness = reader.ReadDouble();

      ElevationRangeDesignID = reader.ReadGuid() ?? Guid.Empty;

      ElevationRangeIsInitialised = reader.ReadBoolean();
      ElevationRangeIsLevelAndThicknessOnly = reader.ReadBoolean();

      ElevationRangeTopElevationForCell = reader.ReadDouble();
      ElevationRangeBottomElevationForCell = reader.ReadDouble();

      LayerState = (LayerState)reader.ReadInt();
      LayerID = reader.ReadInt();

      RestrictFilteredDataToCompactorsOnly = reader.ReadBoolean();

      int surveyedSurfaceCount = reader.ReadInt();
      if (surveyedSurfaceCount > 0)
      {
        SurveyedSurfaceExclusionList = new Guid[surveyedSurfaceCount];
        for (int i = 0; i < SurveyedSurfaceExclusionList.Length; i++)
          SurveyedSurfaceExclusionList[i] = reader.ReadGuid() ?? Guid.Empty;
      }

      MaterialTemperatureMin = (ushort)reader.ReadInt();
      MaterialTemperatureMax = (ushort)reader.ReadInt();
      PasscountRangeMin = (ushort)reader.ReadInt();
      PasscountRangeMax = (ushort)reader.ReadInt();
    }

    /// <summary>
    /// Override equality comparision function with a protected access
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected bool Equals(CellPassAttributeFilterModel other)
    {
      return RequestedGridDataType == other.RequestedGridDataType &&
             HasTimeFilter == other.HasTimeFilter &&
             HasMachineFilter == other.HasMachineFilter &&
             HasMachineDirectionFilter == other.HasMachineDirectionFilter &&
             HasDesignFilter == other.HasDesignFilter &&
             HasVibeStateFilter == other.HasVibeStateFilter &&
             HasLayerStateFilter == other.HasLayerStateFilter &&
             HasMinElevMappingFilter == other.HasMinElevMappingFilter &&
             HasElevationTypeFilter == other.HasElevationTypeFilter &&
             HasGCSGuidanceModeFilter == other.HasGCSGuidanceModeFilter &&
             HasGPSAccuracyFilter == other.HasGPSAccuracyFilter &&
             HasGPSToleranceFilter == other.HasGPSToleranceFilter &&
             HasPositioningTechFilter == other.HasPositioningTechFilter &&
             HasLayerIDFilter == other.HasLayerIDFilter &&
             HasElevationRangeFilter == other.HasElevationRangeFilter &&
             HasPassTypeFilter == other.HasPassTypeFilter &&
             HasCompactionMachinesOnlyFilter == other.HasCompactionMachinesOnlyFilter &&
             HasTemperatureRangeFilter == other.HasTemperatureRangeFilter &&
             FilterTemperatureByLastPass == other.FilterTemperatureByLastPass &&
             HasPassCountRangeFilter == other.HasPassCountRangeFilter &&
             StartTime.Equals(other.StartTime) &&
             EndTime.Equals(other.EndTime) &&

             GuidExtensions.GuidsEqual(MachinesList, other.MachinesList) &&

             DesignNameID == other.DesignNameID &&
             VibeState == other.VibeState &&
             MachineDirection == other.MachineDirection &&
             PassTypeSet == other.PassTypeSet &&
             MinElevationMapping == other.MinElevationMapping &&
             PositioningTech == other.PositioningTech &&
             GPSTolerance == other.GPSTolerance &&
             GPSAccuracyIsInclusive == other.GPSAccuracyIsInclusive &&
             GPSAccuracy == other.GPSAccuracy &&
             GPSToleranceIsGreaterThan == other.GPSToleranceIsGreaterThan &&
             ElevationType == other.ElevationType &&
             GCSGuidanceMode == other.GCSGuidanceMode &&
             ReturnEarliestFilteredCellPass == other.ReturnEarliestFilteredCellPass &&
             ElevationRangeLevel.Equals(other.ElevationRangeLevel) &&
             ElevationRangeOffset.Equals(other.ElevationRangeOffset) &&
             ElevationRangeThickness.Equals(other.ElevationRangeThickness) &&
             ElevationRangeDesignID.Equals(other.ElevationRangeDesignID) &&
             ElevationRangeIsInitialised == other.ElevationRangeIsInitialised &&
             ElevationRangeIsLevelAndThicknessOnly == other.ElevationRangeIsLevelAndThicknessOnly &&
             ElevationRangeTopElevationForCell.Equals(other.ElevationRangeTopElevationForCell) &&
             ElevationRangeBottomElevationForCell.Equals(other.ElevationRangeBottomElevationForCell) &&
             LayerState == other.LayerState &&
             LayerID == other.LayerID &&
             RestrictFilteredDataToCompactorsOnly == other.RestrictFilteredDataToCompactorsOnly &&

             SurveyedSurfaceExclusionList.GuidsEqual(other.SurveyedSurfaceExclusionList) &&

             MaterialTemperatureMin == other.MaterialTemperatureMin &&
             MaterialTemperatureMax == other.MaterialTemperatureMax &&
             PasscountRangeMin == other.PasscountRangeMin &&
             PasscountRangeMax == other.PasscountRangeMax;
    }

    /// <summary>
    /// Equality comparision function with a public access.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ICellPassAttributeFilterModel other)
    {
      return Equals(other as CellPassAttributeFilterModel);
    }

    /// <summary>
    /// Overrides generic object equals implementation to call custom implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((CellPassAttributeFilterModel)obj);
    }

    /// <summary>
    /// Gets hash code.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (int)RequestedGridDataType;
        hashCode = (hashCode * 397) ^ HasTimeFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasMachineFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasMachineDirectionFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasDesignFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasVibeStateFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasLayerStateFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasMinElevMappingFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasElevationTypeFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasGCSGuidanceModeFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasGPSAccuracyFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasGPSToleranceFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasPositioningTechFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasLayerIDFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasElevationRangeFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasPassTypeFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasCompactionMachinesOnlyFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ HasTemperatureRangeFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ FilterTemperatureByLastPass.GetHashCode();
        hashCode = (hashCode * 397) ^ HasPassCountRangeFilter.GetHashCode();
        hashCode = (hashCode * 397) ^ StartTime.GetHashCode();
        hashCode = (hashCode * 397) ^ EndTime.GetHashCode();
        hashCode = (hashCode * 397) ^ (MachinesList != null ? MachinesList.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ DesignNameID;
        hashCode = (hashCode * 397) ^ (int)VibeState;
        hashCode = (hashCode * 397) ^ (int)MachineDirection;
        hashCode = (hashCode * 397) ^ (int)PassTypeSet;
        hashCode = (hashCode * 397) ^ MinElevationMapping.GetHashCode();
        hashCode = (hashCode * 397) ^ (int)PositioningTech;
        hashCode = (hashCode * 397) ^ GPSTolerance.GetHashCode();
        hashCode = (hashCode * 397) ^ GPSAccuracyIsInclusive.GetHashCode();
        hashCode = (hashCode * 397) ^ (int)GPSAccuracy;
        hashCode = (hashCode * 397) ^ GPSToleranceIsGreaterThan.GetHashCode();
        hashCode = (hashCode * 397) ^ (int)ElevationType;
        hashCode = (hashCode * 397) ^ (int)GCSGuidanceMode;
        hashCode = (hashCode * 397) ^ ReturnEarliestFilteredCellPass.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationRangeLevel.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationRangeOffset.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationRangeThickness.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationRangeDesignID.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationRangeIsInitialised.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationRangeIsLevelAndThicknessOnly.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationRangeTopElevationForCell.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationRangeBottomElevationForCell.GetHashCode();
        hashCode = (hashCode * 397) ^ (int)LayerState;
        hashCode = (hashCode * 397) ^ LayerID;
        hashCode = (hashCode * 397) ^ RestrictFilteredDataToCompactorsOnly.GetHashCode();
        hashCode = (hashCode * 397) ^ (SurveyedSurfaceExclusionList != null ? SurveyedSurfaceExclusionList.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ MaterialTemperatureMin.GetHashCode();
        hashCode = (hashCode * 397) ^ MaterialTemperatureMax.GetHashCode();
        hashCode = (hashCode * 397) ^ PasscountRangeMin.GetHashCode();
        hashCode = (hashCode * 397) ^ PasscountRangeMax.GetHashCode();
        return hashCode;
      }
    }
  }
}
