using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.Analytics.CutFillStatistics;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CutFillStatistics.GridFabric
{
  public class CutFillStatisticsResponseTests
  {
    [Fact]
    public void AggregateWith()
    {
      var response = new CutFillStatisticsResponse
      {
        Counts = new long[] {1, 2, 3, 4, 5, 6, 7},
        CellsScannedAtTarget = 10,
        CellsScannedUnderTarget = 11, 
        CellsScannedOverTarget =  12
      };

      var otherResponse = new CutFillStatisticsResponse
      {
        Counts = new long[] { 1, 2, 3, 4, 5, 6, 7 },
        CellsScannedAtTarget = 10,
        CellsScannedUnderTarget = 11,
        CellsScannedOverTarget = 12
      };

      response.AggregateWith(otherResponse);
      response.Counts.Should().BeEquivalentTo(new long[] {2, 4, 6, 8, 10, 12, 14});
      response.CellsScannedAtTarget.Should().Be(20);
      response.CellsScannedUnderTarget.Should().Be(22);
      response.CellsScannedOverTarget.Should().Be(24);
    }

    [Fact]
    public void ConstructResult()
    {
      var response = new CutFillStatisticsResponse
      {
        Counts = new long[] { 1, 2, 3, 4, 5, 6, 7 },
        ResultStatus = RequestErrorStatus.OK
      };

      var result = response.ConstructResult();
      result.Counts.Should().BeEquivalentTo(new long[] { 1, 2, 3, 4, 5, 6, 7 });
      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
    }
  }
}
