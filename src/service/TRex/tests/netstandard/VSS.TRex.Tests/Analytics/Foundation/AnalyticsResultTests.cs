using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Foundation
{
  public class AnalyticsResultTests
  {
    [Fact]
    public void Test_AnalyticsResult_Creation()
    {
      AnalyticsResult r = new AnalyticsResult();

      Assert.True(r.ResultStatus == RequestErrorStatus.Unknown, "Unexpected initialisaton state");
    }
  }
}
