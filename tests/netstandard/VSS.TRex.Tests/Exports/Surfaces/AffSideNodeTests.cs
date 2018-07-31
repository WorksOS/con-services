using VSS.TRex.Exports.Surfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
{
    public class AffSideNodeTests
    {
      [Fact]
      public void AffSideNodeTests_Creation()
      {
        AffSideNode node = new AffSideNode();

        Assert.Null(node.tri);
        Assert.True(node.Next == 0);
        Assert.Null(node.point);
        Assert.Null(node.side);
      }
  }
}
