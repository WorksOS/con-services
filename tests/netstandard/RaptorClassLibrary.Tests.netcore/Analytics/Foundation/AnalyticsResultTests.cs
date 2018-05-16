using VSS.TRex.Analytics.Models;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Foundation
{
    public class AnalyticsResultTests
    {
        [Fact]
        public void Test_AnalyticsResult_Creation()
        {
            AnalyticsResult<int> r = new AnalyticsResult<int>();

            Assert.True(r.ResultStatus == Types.RequestErrorStatus.Unknown, "Unexpected initialisaton state");
        }

        [Fact]
        public void Test_AnalyticsResult_PopulateFromClusterComputeResponse()
        {
            AnalyticsResult<int> r = new AnalyticsResult<int>();
            r.PopulateFromClusterComputeResponse(42);

            Assert.True(r.ResultStatus == Types.RequestErrorStatus.OK, "PopulateFromClusterComputeResponse did not result in OK state");
        }
    }
}
