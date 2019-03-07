using System;
using System.Linq;
using App.Metrics.AspNetCore.Health;
using FluentAssertions;
using VSS.TRex.Cells;
using Xunit;

namespace VSS.TRex.Tests.Cells
{
  public class CellStaticTests
  {
    [Fact]
    public void Creation()
    {
      var cell = new Cell_Static();
      cell.PassCount.Should().Be(0);
      cell.CellPassOffset.Should().Be(0);
      cell.IsEmpty.Should().Be(true);

      cell = new Cell_Static(123, 456);
      cell.PassCount.Should().Be(456);
      cell.CellPassOffset.Should().Be(123);
      cell.IsEmpty.Should().Be(false);
    }

    [Fact]
    public void TopMostHeight()
    {
      var baseTime = DateTime.UtcNow;
      var cell = new Cell_Static
      {
        CellPassOffset = 0,
        PassCount = 10
      };

      var cellPasses = Enumerable.Range(0, 10).Select(x => new CellPass
      {
        Time = baseTime.AddSeconds(x),
        Height = 100 + x
      }).ToArray();

      cell.TopMostHeight(cellPasses).Should().Be(100 + 10 - 1);
    }

    [Fact]
    public void LocateTime()
    {
      var baseTime = DateTime.UtcNow;
      var cell = new Cell_Static
      {
        CellPassOffset = 0,
        PassCount = 10
      };

      var cellPasses = Enumerable.Range(0, 10).Select(x => new CellPass
      {
        Time = baseTime.AddSeconds(x),
        Height = 100 + x
      }).ToArray();

      cell.LocateTime(cellPasses, baseTime.AddSeconds(-1), out int index).Should().Be(false);

      for (int i = 0; i < cellPasses.Length; i++)
      {
        cell.LocateTime(cellPasses, cellPasses[i].Time, out index).Should().Be(true);
        index.Should().Be(i);
      }
    }
  }
}
