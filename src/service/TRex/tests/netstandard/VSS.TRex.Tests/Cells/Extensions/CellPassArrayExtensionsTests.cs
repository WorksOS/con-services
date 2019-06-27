using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Cells.Extensions;
using Xunit;

namespace VSS.TRex.Tests.Cells.Extensions
{
  public class CellPassArrayExtensionsTests
  {
    [Fact]
    public void MinimumTime_AllOrdered()
    {
      var baseTime = DateTime.Now;

      CellPass[] passes =
      {
        new CellPass {Time = baseTime},
        new CellPass {Time = baseTime.AddMinutes(1)},
        new CellPass {Time = baseTime.AddMinutes(2)},
        new CellPass {Time = baseTime.AddMinutes(3)},
        new CellPass {Time = baseTime.AddMinutes(4)},
        new CellPass {Time = baseTime.AddMinutes(5)},
      };

      passes.MinimumTime(0, passes.Length - 1).Should().Be(baseTime);
      passes.MinimumTime(0, passes.Length - 1).Should().Be(passes.Min(x => x.Time));
    }

    [Fact]
    public void MinimumTime_AllUnOrdered()
    {
      var baseTime = DateTime.Now;

      CellPass[] passes =
      {
        new CellPass {Time = baseTime},
        new CellPass {Time = baseTime.AddMinutes(-1)},
        new CellPass {Time = baseTime.AddMinutes(-2)},
        new CellPass {Time = baseTime.AddMinutes(-3)},
        new CellPass {Time = baseTime.AddMinutes(-4)},
        new CellPass {Time = baseTime.AddMinutes(-5)},
      };

      passes.MinimumTime(0, passes.Length - 1).Should().Be(baseTime.AddMinutes(-5));
      passes.MinimumTime(0, passes.Length - 1).Should().Be(passes.Min(x => x.Time));
    }

    [Fact]
    public void MinimumTime_Random()
    {
      var baseTime = DateTime.Now;

      CellPass[] passes =
      {
        new CellPass {Time = baseTime.AddMinutes(-2)},
        new CellPass {Time = baseTime.AddMinutes(1)},
        new CellPass {Time = baseTime.AddMinutes(-2)},
        new CellPass {Time = baseTime.AddMinutes(-10)},
        new CellPass {Time = baseTime},
        new CellPass {Time = baseTime.AddMinutes(5)},
      };

      passes.MinimumTime(0, passes.Length - 1).Should().Be(baseTime.AddMinutes(-10));
      passes.MinimumTime(0, passes.Length - 1).Should().Be(passes.Min(x => x.Time));
    }

    [Fact]
    public void MaxInternalSiteModelMachineIndex_AllOrdered()
    {
      CellPass[] passes =
      {
        new CellPass {InternalSiteModelMachineIndex = 0},
        new CellPass {InternalSiteModelMachineIndex = 1},
        new CellPass {InternalSiteModelMachineIndex = 2},
        new CellPass {InternalSiteModelMachineIndex = 3},
        new CellPass {InternalSiteModelMachineIndex = 4},
        new CellPass {InternalSiteModelMachineIndex = 5},
      };

      passes.MaxInternalSiteModelMachineIndex(0, passes.Length - 1).Should().Be(5);
      passes.MaxInternalSiteModelMachineIndex(0, passes.Length - 1).Should().Be(passes.Max(x => x.InternalSiteModelMachineIndex));
    }

    [Fact]
    public void MaxInternalSiteModelMachineIndex_AllUnOrdered()
    {
      CellPass[] passes =
      {
        new CellPass {InternalSiteModelMachineIndex = 5},
        new CellPass {InternalSiteModelMachineIndex = 4},
        new CellPass {InternalSiteModelMachineIndex = 3},
        new CellPass {InternalSiteModelMachineIndex = 2},
        new CellPass {InternalSiteModelMachineIndex = 1},
        new CellPass {InternalSiteModelMachineIndex = 0},
      };

      passes.MaxInternalSiteModelMachineIndex(0, passes.Length - 1).Should().Be(5);
      passes.MaxInternalSiteModelMachineIndex(0, passes.Length - 1).Should().Be(passes.Max(x => x.InternalSiteModelMachineIndex));
    }

    [Fact]
    public void MaxInternalSiteModelMachineIndex_Random()
    {
      CellPass[] passes =
      {
        new CellPass {InternalSiteModelMachineIndex = 5},
        new CellPass {InternalSiteModelMachineIndex = 4},
        new CellPass {InternalSiteModelMachineIndex = 3},
        new CellPass {InternalSiteModelMachineIndex = 2},
        new CellPass {InternalSiteModelMachineIndex = 1},
        new CellPass {InternalSiteModelMachineIndex = 0},
      };

      passes.MaxInternalSiteModelMachineIndex(0, passes.Length - 1).Should().Be(5);
      passes.MaxInternalSiteModelMachineIndex(0, passes.Length - 1).Should().Be(passes.Max(x => x.InternalSiteModelMachineIndex));
    }
  }
}
