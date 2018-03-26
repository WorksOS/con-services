using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Analytics.Aggregators;
using VSS.VisionLink.Raptor.Analytics.Models;
using Xunit;

namespace VSS.VisionLink.Raptor.Tests.Analytics.Foundation
{
    public class AggregatorBaseTests
    {
        private bool AggregatorStateIsDefault(AggregatorBase Aggregator)
        {
            return Aggregator.CellSize == 0 &&
                Aggregator.CellsScannedAtTarget == 0 &&
                Aggregator.CellsScannedOverTarget == 0 &&
                Aggregator.CellsScannedUnderTarget == 0 &&
                Aggregator.IsTargetValueConstant == true &&
                Aggregator.MissingTargetValue == false &&
                Aggregator.RequiresSerialisation == false &&
                Aggregator.SiteModelID == 0 &&
                Aggregator.SummaryCellsScanned == 0 &&
                Aggregator.SummaryProcessedArea == 0.0 &&
                Aggregator.ValueAtTargetPercent == 0.0 &&
                Aggregator.ValueOverTargetPercent == 0.0 &&
                Aggregator.ValueUnderTargetPercent == 0;
        }

        [Fact]
        public void Test_AggregatorBase_Creation()
        {
            AggregatorBase Aggregator = new AggregatorBase();

            Assert.True(AggregatorStateIsDefault(Aggregator), "Unexpected initialisation state");               
        }

        [Fact]
        public void Test_AggregatorBase_Aggregation()
        {
            // Test base level aggregation
            AggregatorBase Aggregator1 = new AggregatorBase();
            AggregatorBase Aggregator2 = new AggregatorBase();

            Aggregator1.AggregateWith(Aggregator2);
            Assert.True(AggregatorStateIsDefault(Aggregator1), "Unexpected state after default aggregation on default state");

            Aggregator2.CellSize = 1;
            Aggregator2.CellsScannedAtTarget = 10;
            Aggregator2.CellsScannedOverTarget = 20;
            Aggregator2.CellsScannedUnderTarget = 30;

            Aggregator2.SummaryCellsScanned = 60;
            Aggregator2.IsTargetValueConstant = false;
            Aggregator2.MissingTargetValue = true;

            Aggregator1.AggregateWith(Aggregator2);

            Assert.True(Aggregator1.CellSize == 1.0, "Cell size incorrect");
            Assert.True(Aggregator1.CellsScannedAtTarget == 10, "CellsScannedAtTarget incorrect");
            Assert.True(Aggregator1.CellsScannedOverTarget == 20, "CellsScannedOverTarget incorrect");
            Assert.True(Aggregator1.CellsScannedUnderTarget == 30, "CellsScannedUnderTarget incorrect");

            Assert.False(Aggregator1.IsTargetValueConstant, "IsTargetValueConstant incorrect");
            Assert.False(Aggregator1.MissingTargetValue, "MissingTargetValue incorrect");

            Assert.True(Aggregator1.SummaryCellsScanned == 60, "SummaryCellsScanned incorrect");
            Assert.True(Aggregator1.SummaryProcessedArea == 60.0, "SummaryCellsScanned incorrect");
            Assert.True(Math.Abs(Aggregator1.ValueAtTargetPercent - (10 / 60)) < 0.001, "ValueAtTargetPercent  incorrect");
            Assert.True(Math.Abs(Aggregator1.ValueOverTargetPercent- (20 / 60)) < 0.001, "ValueOverTargetPercent incorrect");
            Assert.True(Math.Abs(Aggregator1.ValueUnderTargetPercent - (30 / 60)) < 0.001, "ValueUnderTargetPercent incorrect");
        }
    }
}
