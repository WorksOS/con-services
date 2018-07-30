using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
{
    public class TinningUtilsTests
    {
      [Fact]
      public void TinningUtilsTests_DefinitelyLeftOfBaseline_Positive()
      {
         Assert.True(TinningUtils.DefinitelyLeftOfBaseLine(new TriVertex(0, 0, 0), new TriVertex(1, -1, 0), new TriVertex(1, 1, 0)));
      }

      [Fact]
      public void TinningUtilsTests_DefinitelyLeftOfBaseline_Negative()
      {
        Assert.False(TinningUtils.DefinitelyLeftOfBaseLine(new TriVertex(2, 0, 0), new TriVertex(1, -1, 0), new TriVertex(1, 1, 0)));
      }
  }
}
