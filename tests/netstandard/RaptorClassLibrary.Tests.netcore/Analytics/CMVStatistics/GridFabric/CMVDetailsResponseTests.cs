using System;
using VSS.TRex.Analytics.CMVStatistics.GridFabric.Details;
using VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics.GridFabric
{
  public class CMVDetailsResponseTests : BaseTests
  {
    private CMVDetailsResponse _response => new CMVDetailsResponse()
    {
      ResultStatus = RequestErrorStatus.OK,
      Counts = new long[]{52, 15, 24, 35, 5, 84, 125 }
    };

    [Fact]
    public void Test_CMVDetailsResponse_Creation()
    {
      var response = new CMVDetailsResponse();

      Assert.True(response.ResultStatus == RequestErrorStatus.Unknown, "ResultStatus invalid after creation.");
      Assert.True(response.Counts == null, "Invalid initial value for Counts.");
    }

    [Fact]
    public void Test_CMVDetailsResponse_ConstructResult_Successful()
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
    public void Test_CMVDetailsResponse_AgregateWith_Successful()
    {
      var responseClone = new CMVDetailsResponse()
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
