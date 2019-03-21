using FluentAssertions;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces.GridDecimator;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
    public class TinningEngineTests: IClassFixture<DILoggingFixture>
    {
      [Fact]
      public void TinningEngineTetss_Creation()
      {
        TinningEngine engine = new TinningEngine();

        Assert.NotNull(engine);
        Assert.NotNull(engine.TIN);
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

      [Fact]
      public void BuldTINMesh()
      {
        // Build a very simple TIN from three vertices describing a right hand triangle in the 
        // positive NE quadrant at the origin

        TinningEngine engine = new TinningEngine();
        engine.TIN.Vertices.InitPointSearch(-10, -10, 20, 20, 3);

        var vertex0 = engine.TIN.Vertices.AddPoint(0, 0, 0);
        var vertex1 = engine.TIN.Vertices.AddPoint(0, 10, 0);
        var vertex2 = engine.TIN.Vertices.AddPoint(10, 0, 0);

        engine.BuildTINMesh().Should().BeTrue();

        engine.TIN.Vertices.Count.Should().Be(3);
        engine.TIN.Triangles.Count.Should().Be(1);
      //  engine.TIN.Triangles[0].Vertices.Should().Contain(new [] {vertex0, vertex1, vertex2});
      }
  }
}
