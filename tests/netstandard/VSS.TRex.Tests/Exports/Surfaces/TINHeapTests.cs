using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces
{
    public class TINHeapTests
    {
      [Fact]
      public void TINHeapTests_Creation()
      {
        TINHeap heap = new TINHeap();

        Assert.True(heap != null);
      }

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

        GridToTINTriangle tri1 = new GridToTINTriangle(null, null, null);
        GridToTINTriangle tri2 = new GridToTINTriangle(null, null, null);

        heap.Insert(tri1, 12.34);
        heap.Insert(tri2, 34.56);

        Assert.True(heap.Count == 2);

        Assert.True(heap[0].Tri == tri1);
        Assert.True(heap[1].Tri == tri2);
    }

    [Fact]
      public void TINHeapTests_Update()
      {
        TINHeap heap = new TINHeap(1234);

        GridToTINTriangle tri1 = new GridToTINTriangle(null, null, null);
        GridToTINTriangle tri2 = new GridToTINTriangle(null, null, null);

        heap.Insert(tri1, 12.34);
        heap.Insert(tri2, 34.56);

        Assert.True(heap.Count == 2);

        Assert.True(heap[0].Tri == tri1);
        Assert.True(heap[1].Tri == tri2);

        heap.Update(tri1, 56.78);

        Assert.True(heap[1].Tri == tri1);
        Assert.True(heap[0].Tri == tri2);
    }

      [Fact]
      public void TINHeapTests_Extract()
      {
        TINHeap heap = new TINHeap(1234);

        GridToTINTriangle tri1 = new GridToTINTriangle(null, null, null);
        GridToTINTriangle tri2 = new GridToTINTriangle(null, null, null);

        heap.Insert(tri1, 12.34);
        heap.Insert(tri2, 34.56);

        Assert.True(heap.Extract().Import == 34.56);
      }

      [Fact]
      public void TINHeapTests_Kill()
      {
        TINHeap heap = new TINHeap(1234);

        GridToTINTriangle tri1 = new GridToTINTriangle(null, null, null);
        GridToTINTriangle tri2 = new GridToTINTriangle(null, null, null);

        heap.Insert(tri1, 12.34);
        heap.Insert(tri2, 34.56);

        heap.Kill(1);

        Assert.True(heap.Count == 1);
        Assert.True(heap[0].Tri == tri1);
      }

      [Fact]
      public void TINHeapTests_Top()
      {
        TINHeap heap = new TINHeap(1234);

        GridToTINTriangle tri1 = new GridToTINTriangle(null, null, null);
        GridToTINTriangle tri2 = new GridToTINTriangle(null, null, null);

        heap.Insert(tri1, 12.34);
        heap.Insert(tri2, 34.56);

        Assert.True(heap.Top == heap[0]);
        Assert.True(heap.Top.Tri == tri1);
      }

      [Fact]
      public void TINHeapTests_CheckConsistency()
    {
        TINHeap heap = new TINHeap(1234);

        GridToTINTriangle tri1 = new GridToTINTriangle(null, null, null);
        GridToTINTriangle tri2 = new GridToTINTriangle(null, null, null);

        heap.CheckConsistency();
        Assert.True(true);
      }

      [Fact]
      public void TINHeapTests_CheckConsistency2()
      {
        TINHeap heap = new TINHeap(1234);

        GridToTINTriangle tri1 = new GridToTINTriangle(null, null, null);
        GridToTINTriangle tri2 = new GridToTINTriangle(null, null, null);

        foreach (var node in heap)
          heap.CheckConsistency2(node.Tri);

        Assert.True(true);
      }
    }
}
