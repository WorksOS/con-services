using VSS.TRex.Exports.Surfaces.GridDecimator;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
    public class TriListNodeTests
    {
      [Fact]
      public void TriListNodeTests_Creation()
      {
        TriListNode node = new TriListNode();

        Assert.True(node.Tri == null);
        Assert.True(node.NotAffected == false);
      }
    }
}
