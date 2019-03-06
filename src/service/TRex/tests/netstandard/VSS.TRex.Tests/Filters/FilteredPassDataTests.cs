using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters.Models;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Filters
{
  public class FilteredPassDataTests
  {
    public static CellTargets DummyCellTargets()
    {
      return new CellTargets
      {
        TargetMDP = 10,
        TargetCCA = 11,
        TargetCCV = 12,
        TargetLiftThickness = 13.0f,
        TargetPassCount = 14,
        TempWarningLevelMax = 15,
        TempWarningLevelMin = 16
      };
    }

    public static CellEvents DummyEventValues()
    {
      return new CellEvents
      {
        EventAutoVibrationState = AutoVibrationState.Manual,
        EventDesignNameID = 10,
        EventElevationMappingMode = ElevationMappingMode.MinimumElevation,
        EventFlags = 11,
        EventInAvoidZoneState = 12,
        EventMachineAutomatics = MachineAutomaticsMode.Manual,
        EventMachineGear = MachineGear.Forward3,
        EventMachineRMVThreshold = 100,
        EventVibrationState = VibrationState.Off,
        GPSAccuracy = GPSAccuracy.Medium,
        GPSTolerance = 101,
        LayerID = 102,
        MapReset_DesignNameID = 103,
        MapReset_PriorDate = DateTime.UtcNow,
        PositioningTechnology = PositioningTech.UTS
      };
    }

    public static CellPass DummyCellPass()
    {
      return new CellPass
      {
        Amplitude = 10,
        CCA = 11,
        CCV = 12,
        Frequency = 13,
        GPSModeStore = 14,
        HalfPass = true,
        Height = 15.0f,
        InternalSiteModelMachineIndex = 16,
        MDP = 17,
        MachineSpeed = 18,
        MaterialTemperature = 19,
        PassType = PassType.Rear,
        RMV = 20,
        RadioLatency = 21,
        Time = DateTime.UtcNow,
        gpsMode = GPSMode.DGPS
      };
    }

    public static FilteredPassData DummyFilteredPassData()
    {
      return new FilteredPassData
      {
        MachineType = MachineType.AsphaltCompactor,
        TargetValues = DummyCellTargets(),
        EventValues = DummyEventValues(),
        FilteredPass = DummyCellPass()
      };
    }

    [Fact]
    public void Creation()
    {
      var data = new FilteredPassData();

      data.MachineType.Should().Be(MachineType.Unknown);
    }

    [Fact]
    public void Clear()
    {
      var data = DummyFilteredPassData();
      var data2 = new FilteredPassData();

      data.Clear();
      data2.Clear();

      data.Should().BeEquivalentTo(data2);
    }

    [Fact]
    public void FromToBinary()
    {
      var data = DummyFilteredPassData();

      var writer = new TestBinaryWriter();
      data.ToBinary(writer);

      var data2 = new FilteredPassData();
      data2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      data2.Should().BeEquivalentTo(data2);
    }

    [Fact]
    public void Assign()
    {
      var data = DummyFilteredPassData();
      var data2 = new FilteredPassData();

      data.Assign(data2);

      data2.Should().BeEquivalentTo(data2);
    }
  }
}
