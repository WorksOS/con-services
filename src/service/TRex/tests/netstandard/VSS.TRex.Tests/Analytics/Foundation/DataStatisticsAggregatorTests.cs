using System;
using FluentAssertions;
using VSS.TRex.Analytics.Foundation.Aggregators;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Foundation
{
  public class TestDataStatisticsAggregator : DataStatisticsAggregator
  {
    public void IncrementCountOfTransition(double value) => base.IncrementCountOfTransition(value);

    public long GetMaximumValue() => base.GetMaximumValue();
  }

  public class DataStatisticsAggregatorTests
  {
    private const double Epsilon = 0.00001;

    private bool AggregatorStateIsDefault(DataStatisticsAggregator Aggregator)
    {
      return Math.Abs(Aggregator.CellSize) < Epsilon &&
              Aggregator.CellsScannedAtTarget == 0 &&
              Aggregator.CellsScannedOverTarget == 0 &&
              Aggregator.CellsScannedUnderTarget == 0 &&
              Aggregator.IsTargetValueConstant == true &&
              Aggregator.MissingTargetValue == false &&
              Aggregator.RequiresSerialisation == false &&
              Aggregator.SiteModelID == Guid.Empty &&
              Aggregator.SummaryCellsScanned == 0 &&
              Math.Abs(Aggregator.SummaryProcessedArea) < Epsilon &&
              Math.Abs(Aggregator.ValueAtTargetPercent) < Epsilon &&
              Math.Abs(Aggregator.ValueOverTargetPercent) < Epsilon &&
              Math.Abs(Aggregator.ValueUnderTargetPercent) < Epsilon &&
              Aggregator.DetailsDataValues == null &&
              Aggregator.Counts == null;
    }

    [Fact]
    public void Creation()
    {
      DataStatisticsAggregator aggregator = new DataStatisticsAggregator();

      Assert.True(AggregatorStateIsDefault(aggregator), "Unexpected initialisation state");               
    }

    [Fact]
    public void WithAggregation()
    {
      // Test base level aggregation
      DataStatisticsAggregator aggregator1 = new DataStatisticsAggregator();
      DataStatisticsAggregator aggregator2 = new DataStatisticsAggregator();

      aggregator1.AggregateWith(aggregator2);
      Assert.True(AggregatorStateIsDefault(aggregator1), "Unexpected state after default aggregation on default state");

      aggregator2.CellSize = 1;
      aggregator2.CellsScannedAtTarget = 10;
      aggregator2.CellsScannedOverTarget = 20;
      aggregator2.CellsScannedUnderTarget = 30;

      aggregator2.SummaryCellsScanned = 60;
      aggregator2.IsTargetValueConstant = false;
      aggregator2.MissingTargetValue = true;
      aggregator2.DetailsDataValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      aggregator2.Counts = new long[aggregator2.DetailsDataValues.Length];

      aggregator1.AggregateWith(aggregator2);

      Assert.True(Math.Abs(aggregator1.CellSize - 1) < Epsilon, "Cell size incorrect");
      Assert.True(aggregator1.CellsScannedAtTarget == 10, "CellsScannedAtTarget incorrect");
      Assert.True(aggregator1.CellsScannedOverTarget == 20, "CellsScannedOverTarget incorrect");
      Assert.True(aggregator1.CellsScannedUnderTarget == 30, "CellsScannedUnderTarget incorrect");

      Assert.False(aggregator1.IsTargetValueConstant, "IsTargetValueConstant incorrect");
      Assert.True(aggregator1.MissingTargetValue, "MissingTargetValue incorrect");

      Assert.True(aggregator1.SummaryCellsScanned == 60, "SummaryCellsScanned incorrect");
      Assert.True(Math.Abs(aggregator1.SummaryProcessedArea - 60.0) < Epsilon, "SummaryCellsScanned incorrect");
      Assert.True(Math.Abs(aggregator1.ValueAtTargetPercent - (10 / 60.0) * 100) < Epsilon, "ValueAtTargetPercent  incorrect");
      Assert.True(Math.Abs(aggregator1.ValueOverTargetPercent - (20 / 60.0) * 100) < Epsilon, "ValueOverTargetPercent incorrect");
      Assert.True(Math.Abs(aggregator1.ValueUnderTargetPercent - (30 / 60.0) * 100) < Epsilon, "ValueUnderTargetPercent incorrect");

      Assert.True(aggregator2.Counts.Length == aggregator2.DetailsDataValues.Length, "Invalid value for DetailsDataValues.");
      for (int i = 0; i < aggregator2.Counts.Length; i++)
        Assert.True(aggregator2.Counts[i] == 0, $"Invalid aggregated value for aggregator2.Counts[{i}].");
    }

    [Fact]
    public void Finalise()
    {
      DataStatisticsAggregator aggregator = new DataStatisticsAggregator();

      aggregator.Finalise();

      Assert.True(AggregatorStateIsDefault(aggregator), "Unexpected finalisation state");
    }

    [Fact]
    public void Initialise()
    {
      DataStatisticsAggregator aggregator = new DataStatisticsAggregator();

      aggregator.Initialise(new DataStatisticsAggregator());

      Assert.True(AggregatorStateIsDefault(aggregator), "Unexpected initialisation state");
    }

    [Fact]
    public void ProcessNullSubGridResult()
    {
      DataStatisticsAggregator aggregator = new DataStatisticsAggregator();

      aggregator.ProcessSubGridResult(null);

      aggregator.Finalise();

      Assert.True(AggregatorStateIsDefault(aggregator), "Unexpected finalisation state");
    }

    [Fact]
    public void AggregateWith_SamePassCountTargets()
    {
      var aggregator = new DataStatisticsAggregator
      {
        IsTargetValueConstant = true,
        SummaryCellsScanned = 1
      };

      var otheraggregator = new DataStatisticsAggregator
      {
        IsTargetValueConstant = true,
        SummaryCellsScanned = 1
      };

      aggregator.IsTargetValueConstant.Should().BeTrue();
      otheraggregator.IsTargetValueConstant.Should().BeTrue();

      aggregator.AggregateWith(otheraggregator);
      aggregator.IsTargetValueConstant.Should().BeTrue();
    }

    [Fact]
    public void AggregateWith_DifferingPassCountTargets()
    {
      var aggregator = new DataStatisticsAggregator
      {
        IsTargetValueConstant = true,
        SummaryCellsScanned = 1
      };

      var otheraggregator = new DataStatisticsAggregator
      {
        IsTargetValueConstant = false,
        SummaryCellsScanned = 1
      };

      aggregator.IsTargetValueConstant.Should().BeTrue();
      otheraggregator.IsTargetValueConstant.Should().BeFalse();

      aggregator.AggregateWith(otheraggregator);
      aggregator.IsTargetValueConstant.Should().BeFalse();
    }

    [Fact]
    public void IncrementCountoFTransition()
    {
      var aggregator = new TestDataStatisticsAggregator
      {
        Counts = new long[3],
        DetailsDataValues = new [] {0, 5, 10}
      };

      aggregator.IncrementCountOfTransition(1.0);
      aggregator.Counts[0].Should().Be(1);
    }

    [Fact]
    public void GetMaximumValue()
    {
      var aggregator = new TestDataStatisticsAggregator
      {
        Counts = new long[3],
        DetailsDataValues = new[] { 0, 5, 10 }
      };

      aggregator.GetMaximumValue().Should().Be(0, "because the base statistics aggregator has no GetMaximumValue() implementation");
    }
  }
}
