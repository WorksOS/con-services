using VSS.TRex.Analytics.Models;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Foundation
{
    public class AnalyticsResultTests
    {
        [Fact]
        public void Test_AnalyticsResult_Creation()
        {
            AnalyticsResult r = new AnalyticsResult();

            Assert.True(r.ResultStatus == Types.RequestErrorStatus.Unknown, "Unexpected initialisaton state");
        }
    }
}
