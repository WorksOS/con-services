using VSS.TRex.Exports.Surfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
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
