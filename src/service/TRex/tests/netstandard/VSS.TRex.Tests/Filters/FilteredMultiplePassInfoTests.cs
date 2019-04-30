using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters.Models;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Filters
{
  public class FilteredMultiplePassInfoTests : IClassFixture<DILoggingFixture>
  {
    private CellPass ATestCellPass()
    {
      return new CellPass
      {
        Amplitude = 1,
        CCA = 2,
        CCV = 3,
        Frequency = 4,
        gpsMode = GPSMode.AutonomousPosition,
        HalfPass = false,
        Height = 5,
        //internalSiteModelMachineIndex = 6,
        InternalSiteModelMachineIndex = 6,
        MachineSpeed = 7,
        MaterialTemperature = 8,
        MDP = 9,
        PassType = PassType.Front,
        RadioLatency = 10,
        RMV = 11,
        Time = DateTime.SpecifyKind(new DateTime(2017, 1, 1, 12, 30, 0), DateTimeKind.Utc)
      };
    }

    [Fact]
    public void Creation()
    {
      var info = new FilteredMultiplePassInfo();

      info.PassCount.Should().Be(0);
      info.FilteredPassData.Should().BeNull();
    }

    [Fact]
    public void CreationWithPasses()
    {
      var pass = ATestCellPass();
      var filteredPassData = new FilteredPassData();
      filteredPassData.FilteredPass = pass;

      var info = new FilteredMultiplePassInfo(new [] { filteredPassData });

      info.PassCount.Should().Be(1);
      info.FilteredPassData.Should().NotBeNull();
      info.FilteredPassData[0].Should().BeEquivalentTo(filteredPassData);
    }

    [Fact]
    public void AddOneCellPass_CellPass()
    {
      var info = new FilteredMultiplePassInfo();

      info.AddPass(ATestCellPass());
      info.PassCount.Should().Be(1);
      info.FilteredPassData.Should().NotBeNull();
    }

    [Fact]
    public void AddOneCellPass_FilteredCellPass()
    {
      var info = new FilteredMultiplePassInfo();
      var filteredPassData = new FilteredPassData
      {
        FilteredPass = ATestCellPass()
      };

      info.AddPass(filteredPassData);
      info.PassCount.Should().Be(1);
      info.FilteredPassData.Should().NotBeNull();
    }

    [Fact]
    public void AddCellPassesForcingReallocation()
    {
      var info = new FilteredMultiplePassInfo();

      for (int i = 0; i < Consts.VLPDPSNode_CELLPASSAGG_LISTSIZEINCREMENTDEFAULT + 1; i++)
        info.AddPass(ATestCellPass());
      info.PassCount.Should().Be(Consts.VLPDPSNode_CELLPASSAGG_LISTSIZEINCREMENTDEFAULT + 1);
      info.FilteredPassData.Should().NotBeNull();
    }

    [Fact]
    public void Clear()
    {
      var info = new FilteredMultiplePassInfo();

      info.AddPass(ATestCellPass());
      info.Clear();
      info.PassCount.Should().Be(0);
      info.FilteredPassData.Should().NotBeNull();
    }

    [Fact]
    public void HighestPassTime()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 3).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          Height = 100 - x
        }
      }).ToArray());

      info.HighestPassTime().Should().Be(baseTime);
    }

    [Fact]
    public void LowestPassTime()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 3).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          Height = 100 + x
        }
      }).ToArray());

      info.LowestPassTime().Should().Be(baseTime);
    }

    [Fact]
    public void LastPassValidMDPDetails()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          MDP = (short)x
        },
        TargetValues = new CellTargets
        {
          TargetMDP = (short)(10 + x)
        }
      }).ToArray());

      info.LastPassValidMDPDetails(out short aValue, out short aTarget);
      aValue.Should().Be(1);
      aTarget.Should().Be(11);

      info.FilteredPassData[1].FilteredPass.MDP = CellPassConsts.NullMDP;

      info.LastPassValidMDPDetails(out aValue, out aTarget);
      aValue.Should().Be(0);
      aTarget.Should().Be(10);
    }

    [Fact]
    public void LastPassValidMDPPercentage()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          MDP = (short)(x + 1)
        },
        TargetValues = new CellTargets
        {
          TargetMDP = (short)(10 + x)
        }
      }).ToArray());

      info.LastPassValidMDPPercentage().Should().Be(2 / (10.0 + 1));

      info.FilteredPassData[1].FilteredPass.MDP = CellPassConsts.NullMDP;
      info.FilteredPassData[1].TargetValues.TargetMDP = CellPassConsts.NullMDP;

      info.LastPassValidMDPPercentage().Should().Be(1 / 10.0);

      info.FilteredPassData[0].TargetValues.TargetMDP = 0;
      info.LastPassValidMDPPercentage().Should().Be(CellPassConsts.NullMDPPercentage);

      info.FilteredPassData[0].TargetValues.TargetMDP = CellPassConsts.NullMDP;
      info.LastPassValidMDPPercentage().Should().Be(CellPassConsts.NullMDPPercentage);

      info.FilteredPassData[0].FilteredPass.MDP = CellPassConsts.NullMDP;
      info.LastPassValidMDPPercentage().Should().Be(CellPassConsts.NullMDPPercentage);
    }

    [Fact]
    public void LastPassValidCCADetails()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          CCA = (byte)x
        },
        TargetValues = new CellTargets
        {
          TargetCCA = (byte)(10 + x)
        }
      }).ToArray());

      info.LastPassValidCCADetails(out byte aValue, out byte aTarget);
      aValue.Should().Be(1);
      aTarget.Should().Be(11);

      info.FilteredPassData[1].FilteredPass.CCA = CellPassConsts.NullCCA;

      info.LastPassValidCCADetails(out aValue, out aTarget);
      aValue.Should().Be(0);
      aTarget.Should().Be(10);
    }

    [Fact]
    public void LastPassValidCCVDetails()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          CCV = (short)x
        },
        TargetValues = new CellTargets
        {
          TargetCCV = (short)(10 + x)
        }
      }).ToArray());

      info.LastPassValidCCVDetails(out short aValue, out short aTarget);
      aValue.Should().Be(1);
      aTarget.Should().Be(11);

      info.FilteredPassData[1].FilteredPass.CCV = CellPassConsts.NullMDP;

      info.LastPassValidCCVDetails(out aValue, out aTarget);
      aValue.Should().Be(0);
      aTarget.Should().Be(10);
    }

    [Fact]
    public void LastPassValidCCVPercentage()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          CCV = (short)(x + 1)
        },
        TargetValues = new CellTargets
        {
          TargetCCV = (short)(10 + x)
        }
      }).ToArray());

      info.LastPassValidCCVPercentage().Should().Be(2 / (10.0 + 1));

      info.FilteredPassData[1].FilteredPass.CCV = CellPassConsts.NullCCV;
      info.FilteredPassData[1].TargetValues.TargetCCV = CellPassConsts.NullCCV;

      info.LastPassValidCCVPercentage().Should().Be(1 / 10.0);

      info.FilteredPassData[0].TargetValues.TargetCCV = 0;
      info.LastPassValidCCVPercentage().Should().Be(CellPassConsts.NullCCVPercentage);

      info.FilteredPassData[0].TargetValues.TargetCCV = CellPassConsts.NullCCV;
      info.LastPassValidCCVPercentage().Should().Be(CellPassConsts.NullCCVPercentage);

      info.FilteredPassData[0].FilteredPass.CCV = CellPassConsts.NullCCV;
      info.LastPassValidCCVPercentage().Should().Be(CellPassConsts.NullCCVPercentage);
    }

    [Fact]
    public void FromToBinary()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          CCV = (short)x
        },
        TargetValues = new CellTargets
        {
          TargetCCV = (short)(10 + x)
        },
        EventValues = new CellEvents(),
        MachineType = MachineType.Dozer
      }).ToArray());

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(info);
    }

    [Fact]
    public void LastPassValidMaterialTemperature()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          MaterialTemperature = (ushort)(x + 1)
        }
      }).ToArray());

      info.LastPassValidMaterialTemperature().Should().Be(2);
      info.FilteredPassData[1].FilteredPass.MaterialTemperature = CellPassConsts.NullMaterialTemperatureValue;
      info.LastPassValidMaterialTemperature().Should().Be(1);
      info.FilteredPassData[0].FilteredPass.MaterialTemperature = CellPassConsts.NullMaterialTemperatureValue;
      info.LastPassValidMaterialTemperature().Should().Be(CellPassConsts.NullMaterialTemperatureValue);
    }

    [Fact]
    public void LastPassValidRMV()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          RMV = (short)(x + 1)
        }
      }).ToArray());

      info.LastPassValidRMV().Should().Be(2);
      info.FilteredPassData[1].FilteredPass.RMV = CellPassConsts.NullRMV;
      info.LastPassValidRMV().Should().Be(1);
      info.FilteredPassData[0].FilteredPass.RMV = CellPassConsts.NullRMV;
      info.LastPassValidRMV().Should().Be(CellPassConsts.NullRMV);
    }

    [Fact]
    public void LastPassValidCCV()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          CCV = (short)(x + 1)
        }
      }).ToArray());

      info.LastPassValidCCV().Should().Be(2);
      info.FilteredPassData[1].FilteredPass.CCV = CellPassConsts.NullCCV;
      info.LastPassValidCCV().Should().Be(1);
      info.FilteredPassData[0].FilteredPass.CCV = CellPassConsts.NullCCV;
      info.LastPassValidCCV().Should().Be(CellPassConsts.NullCCV);
    }

    [Fact]
    public void LastPassValidMDP()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          MDP = (short)(x + 1)
        }
      }).ToArray());

      info.LastPassValidMDP().Should().Be(2);
      info.FilteredPassData[1].FilteredPass.MDP = CellPassConsts.NullMDP;
      info.LastPassValidMDP().Should().Be(1);
      info.FilteredPassData[0].FilteredPass.MDP = CellPassConsts.NullMDP;
      info.LastPassValidMDP().Should().Be(CellPassConsts.NullMDP);
    }

    [Fact]
    public void LastPassValidCCA()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          CCA = (byte)(x + 1)
        }
      }).ToArray());

      info.LastPassValidCCA().Should().Be(2);
      info.FilteredPassData[1].FilteredPass.CCA = CellPassConsts.NullCCA;
      info.LastPassValidCCA().Should().Be(1);
      info.FilteredPassData[0].FilteredPass.CCA = CellPassConsts.NullCCA;
      info.LastPassValidCCA().Should().Be(CellPassConsts.NullCCA);
    }

    [Fact]
    public void LastPassValidAmp()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          Amplitude = (ushort)(x + 1)
        }
      }).ToArray());

      info.LastPassValidAmp().Should().Be(2);
      info.FilteredPassData[1].FilteredPass.Amplitude = CellPassConsts.NullAmplitude;
      info.LastPassValidAmp().Should().Be(1);
      info.FilteredPassData[0].FilteredPass.Amplitude = CellPassConsts.NullAmplitude;
      info.LastPassValidAmp().Should().Be(CellPassConsts.NullAmplitude);
    }

    [Fact]
    public void LastPassValidFreq()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          Frequency = (ushort)(x + 1)
        }
      }).ToArray());

      info.LastPassValidFreq().Should().Be(2);
      info.FilteredPassData[1].FilteredPass.Frequency = CellPassConsts.NullFrequency;
      info.LastPassValidFreq().Should().Be(1);
      info.FilteredPassData[0].FilteredPass.Frequency = CellPassConsts.NullFrequency;
      info.LastPassValidFreq().Should().Be(CellPassConsts.NullFrequency);
    }

    [Fact]
    public void LastPassTime()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x)
        }
      }).ToArray());

      info.LastPassTime().Should().Be(baseTime.AddSeconds(1));
    }

    [Fact]
    public void LastPassValidGPSMode()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          gpsMode = x == 0 ? GPSMode.DGPS : GPSMode.AutonomousPosition
        }
      }).ToArray());

      info.LastPassValidGPSMode().Should().Be(GPSMode.AutonomousPosition);
      info.FilteredPassData[1].FilteredPass.gpsMode = CellPassConsts.NullGPSMode;
      info.LastPassValidGPSMode().Should().Be(GPSMode.DGPS);
      info.FilteredPassData[0].FilteredPass.gpsMode = CellPassConsts.NullGPSMode;
      info.LastPassValidGPSMode().Should().Be(CellPassConsts.NullGPSMode);
    }

    [Fact]
    public void LastPassValidRadioLatency()
    {
      var baseTime = DateTime.UtcNow;
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Time = baseTime.AddSeconds(x),
          RadioLatency = (byte)(x + 1)
        }
      }).ToArray());

      info.LastPassValidRadioLatency().Should().Be(2);
      info.FilteredPassData[1].FilteredPass.RadioLatency = CellPassConsts.NullRadioLatency;
      info.LastPassValidRadioLatency().Should().Be(1);
      info.FilteredPassData[0].FilteredPass.RadioLatency = CellPassConsts.NullRadioLatency;
      info.LastPassValidRadioLatency().Should().Be(CellPassConsts.NullRadioLatency);
    }

    [Fact]
    public void Assign()
    {
      var info = new FilteredMultiplePassInfo();

      info.SetFilteredPasses(Enumerable.Range(0, 2).Select(x => FilteredPassDataTests.DummyFilteredPassData()).ToArray());

      var info2 = new FilteredMultiplePassInfo();

      info2.Assign(info);
      info.Should().BeEquivalentTo(info2);
    }
  }
}
