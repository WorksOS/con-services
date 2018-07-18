using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Foundation
{
  public class DetailsAnalyticsResponseTests : BaseTests
  {
    private DetailsAnalyticsResponse _response => new DetailsAnalyticsResponse()
    {
      ResultStatus = RequestErrorStatus.OK,
      Counts = new long[]{52, 15, 24, 35, 5, 84, 125 }
    };

    [Fact]
    public void Test_DetailsAnalyticsResponse_Creation()
    {
      var response = new DetailsAnalyticsResponse();

      Assert.True(response.ResultStatus == RequestErrorStatus.Unknown, "ResultStatus invalid after creation.");
      Assert.True(response.Counts == null, "Invalid initial value for Counts.");
    }

    [Fact]
    public void Test_DetailsAnalyticsResponse_ConstructResult_Successful()
    {
      Assert.True(_response.ResultStatus == RequestErrorStatus.OK, "Invalid initial result status");

      var result = _response.ConstructResult();

      Assert.True(result.ResultStatus == RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");
      Assert.True(result.Counts.Length == _response.Counts.Length, "Invalid initial result value for Counts.Length.");

      for (int i = 0; i < _response.Counts.Length; i++)
        Assert.True(result.Counts[i] == _response.Counts[i], $"Invalid aggregated value for Counts[{i}].");

      Assert.True(result.Percents.Length == result.Counts.Length, "Invalid size of the Counts array.");
    }

    [Fact]
    public void Test_DetailsAnalyticsResponse_AgregateWith_Successful()
    {
      var responseClone = new DetailsAnalyticsResponse()
      {
        ResultStatus = _response.ResultStatus,
        Counts = _response.Counts
      };

      var response = _response.AggregateWith(responseClone);

      Assert.True(response.ResultStatus == _response.ResultStatus, "Invalid aggregated value for ResultStatus.");

      for (int i = 0; i < _response.Counts.Length; i++)
        Assert.True(response.Counts[i] == _response.Counts[i] * 2, $"Invalid aggregated value for Counts[{i}].");
    }
  }
}
