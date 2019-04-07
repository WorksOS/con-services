using System;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Filters
{
  public class CellPassAttributeFilterModelTests
  {
    [Fact]
    public void Creation()
    {
      var _ = new CellPassAttributeFilterModel();
    }

    [Fact]
    public void FromToBinary()
    {
      var data = new CellPassAttributeFilterModel
      {
        GPSAccuracy = GPSAccuracy.Fine,
        MachinesList = new Guid[] {Guid.NewGuid(), Guid.NewGuid() },
        SurveyedSurfaceExclusionList = new Guid[] { Guid.NewGuid(), Guid.NewGuid() },
        LayerID = 1,
        GPSTolerance = 2,
        HasDesignFilter = true,
        DesignNameID = 3,
        ElevationMappingMode = ElevationMappingMode.LatestElevation,
        ElevationRangeDesignUID = Guid.NewGuid(),
        ElevationRangeLevel = 4.0,
        ElevationRangeOffset = 5.0,
        ElevationRangeThickness = 6.0,
        ElevationType = ElevationType.Highest,
        EndTime = Consts.MAX_DATETIME_AS_UTC,
        FilterTemperatureByLastPass = true,
        GCSGuidanceMode = MachineAutomaticsMode.Manual,
        GPSAccuracyIsInclusive = true,
        GPSToleranceIsGreaterThan = true,
        HasCompactionMachinesOnlyFilter = true,
        HasElevationMappingModeFilter = true,
        HasElevationRangeFilter = true,
        HasElevationTypeFilter = true,
        HasGCSGuidanceModeFilter = true,
        HasGPSAccuracyFilter = true,
        HasGPSToleranceFilter = true,
        HasLayerIDFilter = true,
        HasLayerStateFilter = true,
        HasMachineDirectionFilter = true,
        HasMachineFilter = true,
        HasPassCountRangeFilter = true,
        HasPassTypeFilter = true,
        HasPositioningTechFilter = true,
        HasTemperatureRangeFilter = true,
        HasTimeFilter = true,
        HasVibeStateFilter = true,
        LayerState = LayerState.On,
        MachineDirection = MachineDirection.Reverse,
        MaterialTemperatureMax = 10,
        MaterialTemperatureMin = 11,
        PassCountRangeMax = 13,
        PassCountRangeMin = 12,
        PassTypeSet = PassTypeSet.Rear | PassTypeSet.Front,
        PositioningTech = PositioningTech.UTS,
        RequestedGridDataType = GridDataType.CellProfile,
        ReturnEarliestFilteredCellPass = true,
        StartTime = TRex.Common.Consts.MIN_DATETIME_AS_UTC,
        VibeState = VibrationState.On       
      };

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(data);
    }
  }
}
