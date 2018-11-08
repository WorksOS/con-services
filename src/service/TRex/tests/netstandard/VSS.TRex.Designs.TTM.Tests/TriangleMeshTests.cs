using Xunit;

namespace VSS.TRex.Designs.TTM.Tests
{
    public class TriangleMeshTests
    {
      /// <summary>
      /// Constructs a surface containing vertices and triangles that are not linked together through their neighbours
      /// </summary>
      /// <returns></returns>
      private TriangleMesh TinWithTwoTriangles(double atElev)
      {
        TriangleMesh mesh = new TriangleMesh();
        mesh.Vertices.InitPointSearch(-1, -1, 101, 101, 100);

        mesh.Triangles.AddTriangle(mesh.Vertices.AddPoint(0, 0, atElev), mesh.Vertices.AddPoint(0, 100, atElev), mesh.Vertices.AddPoint(100, 0, atElev));
        mesh.Triangles.AddTriangle(mesh.Vertices.AddPoint(100, 0, atElev), mesh.Vertices.AddPoint(100, 100, atElev), mesh.Vertices.AddPoint(0, 100, atElev));

        return mesh;
      }

    [Fact]
      public void Test_TriangleMesh_Creation()
      {
        TriangleMesh mesh = new TriangleMesh();

        Assert.NotNull(mesh);
        Assert.NotNull(mesh.Vertices);
        Assert.NotNull(mesh.Triangles);
      }

      [Fact]
      public void Test_TriangleMesh_GetTriangleAtPoint()
      {
        TriangleMesh mesh = TinWithTwoTriangles(10.0);

        Assert.True(mesh.Triangles.Count == 2);
        Assert.True(mesh.Vertices.Count == 4);

        Triangle tri = mesh.GetTriangleAtPoint(10, 10, out double atElev);

        Assert.True(tri == mesh.Triangles[0], "Incorrect triangle selected (1)");
        Assert.True(atElev == 10.0, "Incorrect interpolation elevation from triangle");

        Triangle tri2 = mesh.GetTriangleAtPoint(90, 90, out atElev);

        Assert.True(tri2 == mesh.Triangles[1], "Incorrect triangle selected (1)");
        Assert.True(atElev == 10.0, "Incorrect interpolation elevation from triangle");
      }

      [Fact]
      public void Test_TriangleMesh_BuildTriangleLinks()
      {
        TriangleMesh mesh = TinWithTwoTriangles(10.0);

        mesh.BuildTriangleLinks();

        Assert.True(mesh.Triangles[0].Neighbours[1] == mesh.Triangles[1]);
        Assert.True(mesh.Triangles[1].Neighbours[2] == mesh.Triangles[0]);
      }

      [Fact]
      public void Test_TriangleMesh_Clear()
      {
        TriangleMesh mesh = TinWithTwoTriangles(10.0);

        Assert.True(mesh.Triangles.Count == 2);
        Assert.True(mesh.Vertices.Count == 4);

        mesh.Clear();

        Assert.True(mesh.Triangles.Count == 0);
        Assert.True(mesh.Vertices.Count == 0);
      }
  }
}
