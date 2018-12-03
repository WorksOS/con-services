using ASNodeRaptorReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApiTests.Compaction.Models
{
  [TestClass]
  public class OffsetStatisticsTests
  {
    private readonly TStation station = new TStation
    {
      CMVAvg = new Random().NextDouble(),
      CMVMax = (short)new Random().Next(),
      CMVMin = (short)new Random().Next(),
      CutFillAvg = new Random().NextDouble(),
      CutFillMax = new Random().NextDouble(),
      CutFillMin = new Random().NextDouble(),
      ElevAvg = new Random().NextDouble(),
      ElevMax = new Random().NextDouble(),
      ElevMin = new Random().NextDouble(),
      MDPAvg = new Random().NextDouble(),
      MDPMax = (short)new Random().NextDouble(),
      MDPMin = (short)new Random().NextDouble(),
      PassCountAvg = new Random().NextDouble(),
      PassCountMax = new Random().Next(),
      PassCountMin = new Random().Next(),
      TemperatureAvg = new Random().NextDouble(),
      TemperatureMax = new Random().NextDouble(),
      TemperatureMin = new Random().NextDouble()
    };

    private readonly CompactionReportStationOffsetRequest request = CompactionReportStationOffsetRequest.CreateRequest(
      0, null, null, 0, null, true, true, true, true, true, true, null, null, 0, 0, 0, null, null, null
    );

    [TestMethod]
    public void Should_create_OffsetStatistics_with_correct_Min_values()
    {
      var statistics = OffsetStatistics.Create(OffsetStatisticType.Minimum, station, request);

      Assert.AreEqual((double)station.CMVMin/10, statistics.CMV);
      Assert.AreEqual(station.CutFillMin, statistics.CutFill);
      Assert.AreEqual(station.ElevMin, statistics.Elevation);
      Assert.AreEqual((double)station.MDPMin/10, statistics.MDP);
      Assert.AreEqual(station.PassCountMin, statistics.PassCount);
      Assert.AreEqual((double)station.TemperatureMin/10, statistics.Temperature);
    }

    [TestMethod]
    public void Should_create_OffsetStatistics_with_correct_Max_values()
    {
      var statistics = OffsetStatistics.Create(OffsetStatisticType.Maximum, station, request);

      Assert.AreEqual((double)station.CMVMax/10, statistics.CMV);
      Assert.AreEqual(station.CutFillMax, statistics.CutFill);
      Assert.AreEqual(station.ElevMax, statistics.Elevation);
      Assert.AreEqual((double)station.MDPMax/10, statistics.MDP);
      Assert.AreEqual(station.PassCountMax, statistics.PassCount);
      Assert.AreEqual((double)station.TemperatureMax/10, statistics.Temperature);
    }

    [TestMethod]
    public void Should_create_OffsetStatistics_with_correct_Avg_values()
    {
      var statistics = OffsetStatistics.Create(OffsetStatisticType.Average, station, request);

      Assert.AreEqual((double)station.CMVAvg/10, statistics.CMV);
      Assert.AreEqual(station.CutFillAvg, statistics.CutFill);
      Assert.AreEqual(station.ElevAvg, statistics.Elevation);
      Assert.AreEqual((double)station.MDPAvg/10, statistics.MDP);
      Assert.AreEqual(station.PassCountAvg, statistics.PassCount);
      Assert.AreEqual((double)station.TemperatureAvg/10, statistics.Temperature);
    }
  }
}
