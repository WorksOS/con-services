using FluentAssertions;
using VSS.TRex.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.GridFabric.Responses
{
  public class SubGridRequestsResponseTests
  {
    [Fact]
    public void Creation()
    {
      var response = new SubGridRequestsResponse();
      response.Should().NotBeNull();
      response.NumProdDataSubGridsExamined.Should().Be(0);
      response.NumProdDataSubGridsProcessed.Should().Be(0);
      response.NumSubgridsExamined.Should().Be(0);
      response.NumSubgridsProcessed.Should().Be(0);
      response.NumSurveyedSurfaceSubGridsExamined.Should().Be(0);
      response.NumSurveyedSurfaceSubGridsProcessed.Should().Be(0);
    }

    [Fact]
    public void Aggregate()
    {
      var response1 = new SubGridRequestsResponse
      {
        NumProdDataSubGridsExamined = 1,
        NumProdDataSubGridsProcessed = 2,
        NumSubgridsExamined = 3,
        NumSubgridsProcessed = 4,
        NumSurveyedSurfaceSubGridsExamined = 5,
        NumSurveyedSurfaceSubGridsProcessed = 6
      };

      var response2 = new SubGridRequestsResponse
      {
        NumProdDataSubGridsExamined = 100,
        NumProdDataSubGridsProcessed = 200,
        NumSubgridsExamined = 300,
        NumSubgridsProcessed = 400,
        NumSurveyedSurfaceSubGridsExamined = 500,
        NumSurveyedSurfaceSubGridsProcessed = 600
      };

      response1.AggregateWith(response2);
      response1.NumProdDataSubGridsExamined.Should().Be(101);
      response1.NumProdDataSubGridsProcessed.Should().Be(202);
      response1.NumSubgridsExamined.Should().Be(303);
      response1.NumSubgridsProcessed.Should().Be(404);
      response1.NumSurveyedSurfaceSubGridsExamined.Should().Be(505);
      response1.NumSurveyedSurfaceSubGridsProcessed.Should().Be(606);

    }
  }
}
