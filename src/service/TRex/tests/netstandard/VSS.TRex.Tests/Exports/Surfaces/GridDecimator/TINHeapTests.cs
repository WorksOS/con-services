using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces.GridDecimator;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
  public class TINHeapTests
  {
    [Fact]
    public void TINHeapTests_CreationWithCapacity()
    {
      TINHeap heap = new TINHeap(1234);

      Assert.True(heap != null);
      Assert.True(heap.Capacity == 1234);
    }

    [Fact]
    public void TINHeapTests_Insert()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(1, 2, 0), new TriVertex(2, 2, 0), new TriVertex(2, 1, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(2, 1, 0), new TriVertex(1, 2, 0), new TriVertex(2, 2, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      Assert.True(heap.Count == 2);

      Assert.True(heap[1].Tri == tri1);
      Assert.True(heap[0].Tri == tri2);
    }

    [Fact]
    public void TINHeapTests_Update()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(1, 2, 0), new TriVertex(2, 2, 0), new TriVertex(2, 1, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(2, 1, 0), new TriVertex(1, 2, 0), new TriVertex(2, 2, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      Assert.True(heap.Count == 2);

      Assert.True(heap[1].Tri == tri1);
      Assert.True(heap[0].Tri == tri2);

      heap.Update(tri1, 56.78);

      Assert.True(heap[0].Tri == tri1);
      Assert.True(heap[1].Tri == tri2);
    }

    [Fact]
    public void TINHeapTests_Extract()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(1, 2, 0), new TriVertex(2, 2, 0), new TriVertex(2, 1, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(2, 1, 0), new TriVertex(1, 2, 0), new TriVertex(2, 2, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      Assert.True(heap.Extract().Import == 34.56);
      Assert.True(heap.Count == 1, "Count not one after extracting element from heap");
    }

    [Fact]
    public void TINHeapTests_Kill_LastNode()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(1, 2, 0), new TriVertex(2, 2, 0), new TriVertex(2, 1, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(2, 1, 0), new TriVertex(1, 2, 0), new TriVertex(2, 2, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      heap.Kill(1);

      Assert.True(heap.Count == 1);
      Assert.True(heap[0].Tri == tri2);
    }

    [Fact]
    public void TINHeapTests_Kill_FirstNode()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(1, 2, 0), new TriVertex(2, 2, 0), new TriVertex(2, 1, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(2, 1, 0), new TriVertex(1, 2, 0), new TriVertex(2, 2, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      heap.Kill(0);

      Assert.True(heap.Count == 1);
      Assert.True(heap[0].Tri == tri1);
    }

    [Fact]
    public void TINHeapTests_Top()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(1, 2, 0), new TriVertex(2, 2, 0), new TriVertex(2, 1, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(2, 1, 0), new TriVertex(1, 2, 0), new TriVertex(2, 2, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      Assert.True(heap.Top == heap[0]);
      Assert.True(heap.Top.Tri == tri2);
    }

    [Fact]
    public void TINHeapTests_CheckConsistency()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(1, 2, 0), new TriVertex(2, 2, 0), new TriVertex(2, 1, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(2, 1, 0), new TriVertex(1, 2, 0), new TriVertex(2, 2, 0));
      GridToTINTriangle tri3 = new GridToTINTriangle(new TriVertex(0, 0, 0), new TriVertex(1, 2, 0), new TriVertex(3, 3, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      // This will succeed
      heap.CheckConsistency();

      // Mangle the HeapIndexes
      foreach (var node in heap)
        node.Tri.HeapIndex = 1000;

      // This will fail
      Action act = () => heap.CheckConsistency();
      act.Should().Throw<TRexException>().WithMessage("Inconsistent heap indexing");

      Assert.True(true);
    }

    [Fact]
    public void TINHeapTests_CheckConsistency2()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(1, 2, 0), new TriVertex(2, 2, 0), new TriVertex(2, 1, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(2, 1, 0), new TriVertex(1, 2, 0), new TriVertex(2, 2, 0));
      GridToTINTriangle tri3 = new GridToTINTriangle(new TriVertex(0, 0, 0), new TriVertex(1, 2, 0), new TriVertex(3, 3, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      // This will succeed
      foreach (var node in heap)
        heap.CheckConsistency2(node.Tri);

      // Mangle the triangel references
//      foreach (var node in heap)
//        node.Tri = tri3;

      // This will fail
      Action act = () => heap.CheckConsistency2(tri3);
      act.Should().Throw<TRexException>().WithMessage("Triangle is not in the heap");

      Assert.True(true);
    }

    [Fact]
    public void TINHeapTests_CheckListConsistency()
    {
      TINHeap heap = new TINHeap(1234);

      GridToTINTriangle tri1 = new GridToTINTriangle(new TriVertex(10, 20, 0), new TriVertex(20, 20, 0), new TriVertex(20, 10, 0));
      GridToTINTriangle tri2 = new GridToTINTriangle(new TriVertex(20, 10, 0), new TriVertex(10, 20, 0), new TriVertex(20, 20, 0));

      heap.Insert(tri1, 12.34);
      heap.Insert(tri2, 34.56);

      // This will fail
      Action act = () => heap.CheckListConsistency();
      act.Should().Throw<TRexException>().WithMessage("Tri does not contain heap location");

      // Move the insertion point outside of a triangle in the heap node
      foreach (var node in heap)
      {
        var centroid = node.Tri.Centroid();
        node.sx = (int)Math.Truncate(centroid.X);
        node.sy = (int)Math.Truncate(centroid.Y);
      }

      // This will succeed
      heap.CheckListConsistency();

      Assert.True(true);
    }
  }
}
