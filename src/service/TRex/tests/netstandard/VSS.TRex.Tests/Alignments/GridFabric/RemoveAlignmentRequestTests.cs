using FluentAssertions;
using VSS.TRex.Alignments.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Alignments.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(RemoveAlignmentRequest))]
  public class RemoveAlignmentRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var req = new RemoveAlignmentRequest();
      req.Should().NotBeNull();
    }
  }
}
