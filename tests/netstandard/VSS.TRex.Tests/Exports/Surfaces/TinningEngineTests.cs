using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
{
    public class TinningEngineTests
    {
      [Fact]
      public void TinningEngineTetss_Creation()
      {
        TinningEngine engine = new TinningEngine();

        Assert.NotNull(engine);
        Assert.NotNull(engine.TIN);
        //Assert.NotNull(engine.Decimator);
      }

      [Fact]
      public void TinningEngineTetss_AddVertex()
      {
        TinningEngine engine = new TinningEngine();

        engine.TIN.Vertices.InitPointSearch(-10, -10, 10, 10, 10);

        TriVertex v =  engine.AddVertex(1, 2, 3);

        Assert.True(v.X == 1);
        Assert.True(v.Y == 2);
        Assert.True(v.Z == 3);

        Assert.True(engine.TIN.Vertices.Count == 1);
        Assert.True(engine.TIN.Vertices[0] == v);
      }

    [Fact]
      public void TinningEngineTetss_AddTriangle()
      {
        TinningEngine engine = new TinningEngine();

        engine.TIN.Vertices.InitPointSearch(-10, -10, 10, 10, 10);

        TriVertex v0 = engine.AddVertex(0, 0, 0);
        TriVertex v1 = engine.AddVertex(1, 0, 0);
        TriVertex v2 = engine.AddVertex(0, 1, 0);

        Triangle t = engine.AddTriangle(v0, v1, v2);

        Assert.True(engine.TIN.Triangles.Count == 1);
        Assert.True(engine.TIN.Triangles[0] == t);
      }
  }
}
