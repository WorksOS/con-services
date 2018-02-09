using ASNodeRaptorReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

    [TestMethod]
    public void Should_create_OffsetStatistics_with_correct_Min_values()
    {
      var statistics = OffsetStatistics.Create(OffsetStatisticType.Minimum, station);

      Assert.AreEqual(station.CMVMin, statistics.CMV);
      Assert.AreEqual(station.CutFillMin, statistics.CutFill);
      Assert.AreEqual(station.ElevMin, statistics.Elevation);
      Assert.AreEqual(station.MDPMin, statistics.MDP);
      Assert.AreEqual(station.PassCountMin, statistics.PassCount);
      Assert.AreEqual(station.TemperatureMin, statistics.Temperature);
    }

    [TestMethod]
    public void Should_create_OffsetStatistics_with_correct_Max_values()
    {
      var statistics = OffsetStatistics.Create(OffsetStatisticType.Maximum, station);

      Assert.AreEqual(station.CMVMax, statistics.CMV);
      Assert.AreEqual(station.CutFillMax, statistics.CutFill);
      Assert.AreEqual(station.ElevMax, statistics.Elevation);
      Assert.AreEqual(station.MDPMax, statistics.MDP);
      Assert.AreEqual(station.PassCountMax, statistics.PassCount);
      Assert.AreEqual(station.TemperatureMax, statistics.Temperature);
    }

    [TestMethod]
    public void Should_create_OffsetStatistics_with_correct_Avg_values()
    {
      var statistics = OffsetStatistics.Create(OffsetStatisticType.Average, station);

      Assert.AreEqual(station.CMVAvg, statistics.CMV);
      Assert.AreEqual(station.CutFillAvg, statistics.CutFill);
      Assert.AreEqual(station.ElevAvg, statistics.Elevation);
      Assert.AreEqual(station.MDPAvg, statistics.MDP);
      Assert.AreEqual(station.PassCountAvg, statistics.PassCount);
      Assert.AreEqual(station.TemperatureAvg, statistics.Temperature);
    }
  }
}