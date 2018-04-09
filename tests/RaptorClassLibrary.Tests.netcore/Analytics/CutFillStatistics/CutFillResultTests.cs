using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;
using VSS.VisionLink.Raptor.Analytics.Models;
using Xunit;

namespace VSS.VisionLink.Raptor.Tests.Analytics.CutFillStatistics
{
    public class CutFillResultTests
    {
        [Fact]
        public void Test_CutFillResult_Population_Successful()
        {
            CutFillResult r = new CutFillResult();
            long[] testCounts = { 100, 100, 100, 400, 100, 100, 100 };
            double[] testPercents = { 10.0, 10.0, 10.0, 40.0, 10.0, 10.0, 10.0};

            Assert.True(r.ResultStatus != Types.RequestErrorStatus.OK, "Invalid initial result status");

            r.PopulateFromClusterComputeResponse(new CutFillStatisticsResponse()
            {
                ResultStatus = Types.RequestErrorStatus.OK,
                Counts = testCounts
            });

            Assert.True(r.ResultStatus == Types.RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");

            for (int i = 0; i < r.Percents.Length; i++)
            {
                Assert.True(Math.Abs(testPercents[i] - r.Percents[i]) < 0.00001, $"Invalid initial result percentage for item {i}");
            }
        }

        [Fact]
        public void Test_CutFillResult_Population_Failure()
        {
            CutFillResult r = new CutFillResult();

            Assert.Throws<ArgumentException>("Response", () => { r.PopulateFromClusterComputeResponse(null); });
            Assert.Throws<ArgumentException>("Response", () => { r.PopulateFromClusterComputeResponse(new object()); });
        }
    }
}
