using FluentAssertions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.QuantizedMesh.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.QuantizedMesh
{
  // Raymonds unit test spys are watching :)
  [UnitTestCoveredRequest(RequestType = typeof(QuantizedMeshRequest))]
  [UnitTestCoveredRequest(RequestType = typeof(QMTileRequest))]
  public class QMTileRequestTests : IClassFixture<DIRenderingFixture>
  {

    [Fact]
    public void Test_QMTileRequest_Creation()
    {
      var request = new QMTileRequest();

      request.Should().NotBeNull();
    }

    [Fact]
    public void Test_QuantizedMeshRequest_Creation()
    {
      var request = new QuantizedMeshRequest();

      request.Should().NotBeNull();
    }

  }

}
