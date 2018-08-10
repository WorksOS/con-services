using System;
using Xunit;

namespace VSS.TRex.Designs.TTM.Optimised.Tests
{
  public class OptimisedTTMPerfTests
  {
    [Fact]
    public void Test_TINLoad()
    {
      VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel tin = new VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel();

      // 165Mb TIN 
      DateTime startTime = DateTime.Now;
      tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
      DateTime endTime = DateTime.Now;
      Assert.True(false, $"Duration to load file containing {tin.Triangles.Items.Length} triangles and {tin.Vertices.Items.Length} vertices: {endTime - startTime}");
    }

    [Fact]
    public void Test_TINLoad2()
    {
      VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel readonly_tin = new VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel();

      // 165Mb TIN 
      readonly_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");

      VSS.TRex.Designs.TTM.TrimbleTINModel readwrite_tin = new VSS.TRex.Designs.TTM.TrimbleTINModel();

      // 165Mb TIN 
      readwrite_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");

      // Check the numbers of vertices, triangles, edges and start points are the same
      Assert.True(readwrite_tin.Triangles.Count == readonly_tin.Triangles.Items.Length, $"Triangle counts do not match: {readwrite_tin.Triangles.Count} vs {readonly_tin.Triangles.Items.Length}");
      Assert.True(readwrite_tin.Vertices.Count == readonly_tin.Vertices.Items.Length, $"Vertex counts do not match: {readwrite_tin.Vertices.Count} vs {readonly_tin.Vertices.Items.Length}");
      Assert.True(readwrite_tin.Edges.Count == readonly_tin.Edges.Items.Length, $"Edge counts do not match: {readwrite_tin.Edges.Count} vs {readonly_tin.Edges.Items.Length}");
      Assert.True(readwrite_tin.StartPoints.Count == readonly_tin.StartPoints.Items.Length, $"StartPoint counts do not match: {readwrite_tin.StartPoints.Count} vs {readonly_tin.StartPoints.Items.Length}");

      // Check the contents of triangles is the same
      for (int i = 0; i < readwrite_tin.Triangles.Count; i++)
      {
//        Assert.True(readwrite_tin.Triangles[i].Tag == readonly_tin.Triangles.Items[i].Tag, $"Triangle TAGs vary at index {i}, {readwrite_tin.Triangles[i].Tag} vs {readonly_tin.Triangles.Items[i].Tag}");
        Assert.True(readwrite_tin.Triangles[i].Vertices[0].Tag == readonly_tin.Triangles.Items[i].Vertex0 + 1, $"Triangle vertex 0 Tags vary at index {i}, {readwrite_tin.Triangles[i].Vertices[0].Tag} vs {readonly_tin.Triangles.Items[i].Vertex0 + 1}");
        Assert.True(readwrite_tin.Triangles[i].Vertices[1].Tag == readonly_tin.Triangles.Items[i].Vertex1 + 1, $"Triangle vertex 1 Tags vary at index {i}, {readwrite_tin.Triangles[i].Vertices[1].Tag} vs {readonly_tin.Triangles.Items[i].Vertex1 + 1}");
        Assert.True(readwrite_tin.Triangles[i].Vertices[2].Tag == readonly_tin.Triangles.Items[i].Vertex2 + 1, $"Triangle vertex 2 Tags vary at index {i}, {readwrite_tin.Triangles[i].Vertices[2].Tag} vs {readonly_tin.Triangles.Items[i].Vertex2 + 1}");
      }

      // Check the contents of vertices are the same
      for (int i = 0; i < readwrite_tin.Vertices.Count; i++)
      {
//        Assert.True(readwrite_tin.Vertices[i].Tag == readonly_tin.Vertices.Items[i].Tag, $"Vertex TAGs vary at index {i}, {readwrite_tin.Vertices[i].Tag} vs {readonly_tin.Vertices.Items[i].Tag}");
        Assert.True(readwrite_tin.Vertices[i].XYZ.Equals(readonly_tin.Vertices.Items[i]), $"Vertex location varies at index {i}, {readwrite_tin.Vertices[i].XYZ} vs {readonly_tin.Vertices.Items[i]}");
      }

      // Check the contents of edges are the same
      for (int i = 0; i < readwrite_tin.Edges.Count; i++)
      {
        Assert.True(readwrite_tin.Edges[i].Tag == readonly_tin.Edges.Items[i] + 1, $"Edges TAGs vary at index {i}, {readwrite_tin.Edges[i].Tag} vs {readonly_tin.Edges.Items[i] + 1}");
      }

      // Check the contents of start points are the same
      for (int i = 0; i < readwrite_tin.StartPoints.Count; i++)
      {
        Assert.True(readwrite_tin.StartPoints[i].Triangle.Tag == readonly_tin.StartPoints.Items[i].Triangle + 1, 
          $"Startpoint TAGs vary at index {i}, {readwrite_tin.StartPoints[i].Triangle.Tag} vs {readonly_tin.StartPoints.Items[i].Triangle + 1}");
      }
    }

    [Fact]
    public void Test_TINLoadAndSave()
    {
      VSS.TRex.Designs.TTM.TrimbleTINModel readwrite_tin = new VSS.TRex.Designs.TTM.TrimbleTINModel();

      // 165Mb TIN 
      readwrite_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");

      readwrite_tin.SaveToFile(@"C:\Temp\5644616_oba9c0bd14_FRL.ttm(write from unit test)");
    }
  }
}
