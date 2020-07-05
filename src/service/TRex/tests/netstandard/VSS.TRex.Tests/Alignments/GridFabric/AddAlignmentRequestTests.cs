using FluentAssertions;
using VSS.TRex.Alignments.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Alignments.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AddAlignmentRequest))]
  public class AddAlignmentRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var req = new AddAlignmentRequest();
      req.Should().NotBeNull();
    }
  }
}
