using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Analytics.Models;
using Xunit;

namespace VSS.VisionLink.Raptor.Tests.Analytics.Foundation
{
    public class AnalyticsResultTests
    {
        [Fact]
        public void Test_AnalyticsResult_Creation()
        {
            AnalyticsResult r = new AnalyticsResult();

            Assert.True(r.ResultStatus == Types.RequestErrorStatus.Unknown, "Unexpected initialisaton state");
        }

        [Fact]
        public void Test_AnalyticsResult_PopulateFromClusterComputeResponse()
        {
            AnalyticsResult r = new AnalyticsResult();
            r.PopulateFromClusterComputeResponse(new object());

            Assert.True(r.ResultStatus == Types.RequestErrorStatus.OK, "PopulateFromClusterComputeResponse did not result in OK state");
        }
    }
}
